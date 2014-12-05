using System.Runtime.Remoting.Contexts;

namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    using Compilation;
    using Compilation.Inspectors;
    using Configuration;
    using Parallel;
    using Text;

    /// <summary>
    /// Defines a template service.
    /// </summary>
    public class TemplateService : MarshalByRefObject, ITemplateService
    {
        #region Fields
        private readonly ITemplateServiceConfiguration _config;

        private readonly ConcurrentDictionary<string, CachedTemplateItem> _cache = new ConcurrentDictionary<string, CachedTemplateItem>();
        private readonly ConcurrentBag<Assembly> _assemblies = new ConcurrentBag<Assembly>();
        
        private readonly TypeLoader _loader;
        private static readonly Type _objectType = typeof(object);

        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="config">The template service configuration.</param>
        public TemplateService(ITemplateServiceConfiguration config)
        {
            Contract.Requires(config != null);

            _config = config;
            _loader = new TypeLoader(AppDomain.CurrentDomain, _assemblies);
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        public TemplateService()
            : this(new TemplateServiceConfiguration()) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">the encoding.</param>
        internal TemplateService(Language language, Encoding encoding)
            : this(new TemplateServiceConfiguration() { Language = language, EncodedStringFactory = GetEncodedStringFactory(encoding) }) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        public ITemplateServiceConfiguration Configuration { get { return _config; } }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get { return _config.EncodedStringFactory; } }

        public bool IncludeDebugInformation { get; set; }

        #endregion

        #region Methods
        public void AddNamespace(string ns)
        {
            _config.Namespaces.Add(ns);
        }

        public void Compile(string razorTemplate, Type modelType, string cacheName, string razorTemplateFilePath = null)
        {
            Contract.Requires(razorTemplate != null);
            Contract.Requires(cacheName != null);

            int hashCode = razorTemplate.GetHashCode();

            Type type = CreateTemplateType(razorTemplate, modelType, razorTemplateFilePath);
            var item = new CachedTemplateItem(hashCode, type);

            _cache.AddOrUpdate(cacheName, item, (n, i) => item);
        }

        /// <summary>
        /// Creates a new <see cref="ExecuteContext"/> for tracking templates.
        /// </summary>
        /// <param name="viewBag">The dynamic view bag.</param>
        /// <returns>The execute context.</returns>
        public virtual ExecuteContext CreateExecuteContext(DynamicViewBag viewBag = null)
        {
            var context = new ExecuteContext(new DynamicViewBag(viewBag));

            return context;
        }

        /// <summary>
        /// Creates a new <see cref="InstanceContext"/> for creating template instances.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>An instance of <see cref="InstanceContext"/>.</returns>
        [Pure]
        protected virtual InstanceContext CreateInstanceContext(Type templateType)
        {
            return new InstanceContext(_loader, templateType);
        }

        [Pure]
        public virtual ITemplate CreateTemplate(string razorTemplate, Type templateType, object model, string razorTemplateFilePath = null)
        {
            if (templateType == null)
            {
                Type modelType = (model == null) ? typeof(object) : model.GetType();
                templateType = CreateTemplateType(razorTemplate, modelType, razorTemplateFilePath);
            }

            var context = CreateInstanceContext(templateType);
            ITemplate instance = _config.Activator.CreateInstance(context);
            instance.TemplateService = this;

            SetModel(instance, model);

            return instance;
        }

        [Pure]
        public virtual IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> templateTypes, IEnumerable<object> models, IEnumerable<string> razorTemplateFilePaths = null, bool parallel = false)
        {
            if ((razorTemplates == null) && (templateTypes == null))
                throw new ArgumentException("The razorTemplates and templateTypes parameters may not both be null.");

            List<string> razorTemplateList = (razorTemplates == null) ? null : razorTemplates.ToList();
            List<object> modelList = (models == null) ? null : models.ToList();
            List<Type> templateTypeList = (templateTypes == null) ? null : templateTypes.ToList();
            List<string> razorTemplateFilePathList = (razorTemplateFilePaths == null) ? null : razorTemplateFilePaths.ToList();

            int templateCount = (razorTemplateList != null) ? razorTemplateList.Count() : templateTypeList.Count();

            if ((razorTemplateList != null) && (razorTemplateList.Count != templateTypeList.Count))
                throw new ArgumentException("Expected the same number of templateTypes as razorTemplates to be processed.");

            if ((templateTypeList != null) && (templateTypeList.Count != templateCount))
                throw new ArgumentException("Expected the same number of templateTypes as the number of templates to be processed.");

            if ((modelList != null) && (modelList.Count != templateCount))
                throw new ArgumentException("Expected the same number of models as the number of templates to be processed.");

            if ((razorTemplateFilePathList != null) && (razorTemplateFilePathList.Count != templateCount))
                throw new ArgumentException("Expected the same number of template filepath as the number of templates to be processed.");

            if ((razorTemplateList != null) && (templateTypeList != null))
            {
                for (int i = 0; (i < templateCount); i++)
                {
                    if (razorTemplateList == null)
                    {
                        if (templateTypeList[i] == null)
                            throw new ArgumentException("Expected non-NULL value in templateTypes[" + i.ToString() + "].");
                    }
                    else if (templateTypeList == null)
                    {
                        if (razorTemplateList[i] == null)
                            throw new ArgumentException("Expected non-NULL value in either razorTemplates[" + i.ToString() + "].");
                    }
                    else
                    {
                        if ((razorTemplateList[i] == null) && (templateTypeList[i] == null))
                            throw new ArgumentException("Expected non-NULL value in either razorTemplates[" + i.ToString() + "] or templateTypes[" + i.ToString() + "].");
                    }
                }
            }

            if (parallel)
            {
                if (razorTemplateList != null)
                    return GetParallelQueryPlan<string>()
                        .CreateQuery(razorTemplates)
                        .Select((rt, i) => CreateTemplate(
                            rt,
                            (templateTypeList == null) ? null : templateTypeList[i],
                            (modelList == null) ? null : modelList[i],
                            (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]
                            ));
                else
                    return GetParallelQueryPlan<Type>()
                        .CreateQuery(templateTypes)
                        .Select((tt, i) => CreateTemplate(
                            (razorTemplateList == null) ? null : razorTemplateList[i],
                            tt,
                            (modelList == null) ? null : modelList[i],
                            (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]
                            ));
            }

            if (razorTemplateList != null)
                return razorTemplates.Select((rt, i) => CreateTemplate(
                    rt,
                    (templateTypeList == null) ? null : templateTypeList[i],
                    (modelList == null) ? null : modelList[i],
                    (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]
                    ));
            else
                return templateTypeList.Select((tt, i) => CreateTemplate(
                    (razorTemplateList == null) ? null : razorTemplateList[i],
                    tt,
                    (modelList == null) ? null : modelList[i],
                    (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]));
        }

        [Pure]
        public virtual Type CreateTemplateType(string razorTemplate, Type modelType, string razorTemplateFilePath = null)
        {
            var context = new TypeContext
                              {
                                  ModelType = (modelType == null) ? typeof(object) : modelType,
                                  TemplateContent = razorTemplate,
                                  TemplateFileName = razorTemplateFilePath,
                                  IncludeDebugInformation = IncludeDebugInformation,
                                  TemplateType = (_config.BaseTemplateType) ?? typeof(TemplateBase<>)
                              };

            foreach (string ns in _config.Namespaces)
                context.Namespaces.Add(ns);

            var service = _config
                .CompilerServiceFactory
                .CreateCompilerService(_config.Language);
            service.Debug = _config.Debug;
            service.CodeInspectors = _config.CodeInspectors ?? Enumerable.Empty<ICodeInspector>();

            var result = service.CompileType(context);

            _assemblies.Add(result.Item2);

            return result.Item1;
        }

        [Pure]
        public virtual IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> modelTypes, IEnumerable<string> razorTemplateFilePaths = null, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);

            List<Type> modelTypeList = (modelTypes == null) ? null : modelTypes.ToList();
            List<string> razorTemplateFileList = (razorTemplateFilePaths == null)
                ? null
                : razorTemplateFilePaths.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => CreateTemplateType(t,
                        (modelTypeList == null) ? null : modelTypeList[i],
                        (razorTemplateFileList == null) ? null : razorTemplateFileList[i]));

            return razorTemplates.Select((t, i) => CreateTemplateType(t,
                (modelTypeList == null) ? null : modelTypeList[i],
                (razorTemplateFileList == null) ? null : razorTemplateFileList[i]));
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing">Are we explicitly disposing of this instance?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                _loader.Dispose();
                disposed = true;
            }
        }

        /// <summary>
        /// Gets an instance of a <see cref="IEncodedStringFactory"/> for a known encoding.
        /// </summary>
        /// <param name="encoding">The encoding to get a factory for.</param>
        /// <returns>An instance of <see cref="IEncodedStringFactory"/></returns>
        internal static IEncodedStringFactory GetEncodedStringFactory(Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.Html:
                    return new HtmlEncodedStringFactory();

                case Encoding.Raw:
                    return new RawStringFactory();

                default:
                    throw new ArgumentException("Unsupported encoding: " + encoding);
            }
        }

        /// <summary>
        /// Gets a parellel query plan used to configure a parallel query.
        /// </summary>
        /// <typeparam name="T">The query item type.</typeparam>
        /// <returns>An instance of <see cref="IParallelQueryPlan{T}"/>.</returns>
        protected virtual IParallelQueryPlan<T> GetParallelQueryPlan<T>()
        {
            return new DefaultParallelQueryPlan<T>();
        }

        public virtual ITemplate GetTemplate(string razorTemplate, object model, string cacheName, string razorTemplateFilePath = null)
        {
            return this.GetTemplate<object>(razorTemplate, model, cacheName, razorTemplateFilePath);
        }

        private ITemplate GetTemplate<T>(string razorTemplate, object model, string cacheName, string razorTemplateFilePath = null)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");

            int hashCode = razorTemplate.GetHashCode();

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(cacheName, out item) && item.CachedHashCode == hashCode))
            {
                Type type = CreateTemplateType(razorTemplate, (model == null) ? typeof(T) : model.GetType(), razorTemplateFilePath);
                item = new CachedTemplateItem(hashCode, type);

                _cache.AddOrUpdate(cacheName, item, (n, i) => item);
            }

            var instance = CreateTemplate(null, item.TemplateType, model, razorTemplateFilePath);
            return instance;
        }

        public virtual IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> cacheNames, IEnumerable<string> razorTemplateFilePaths = null, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(cacheNames != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected same number of models as string templates to be processed.");
            Contract.Requires(razorTemplates.Count() == cacheNames.Count(),
                              "Expected same number of cache names as string templates to be processed.");
            Contract.Requires(razorTemplates.Count() == razorTemplateFilePaths.Count(),
                              "Expected same number of tempate file paths as string templates to be processed.");

            var modelList = (models == null) ? null : models.ToList();
            var razorTemplateFilePathList = (razorTemplateFilePaths == null) ? null : razorTemplateFilePaths.ToList();

            var cacheNameList = cacheNames.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => GetTemplate(t, 
                        (modelList == null) ? null : modelList[i],
                        cacheNameList[i],
                        (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]));

            return razorTemplates.Select((t, i) => GetTemplate(t, 
                (modelList == null) ? null : modelList[i],
                cacheNameList[i],
                (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]));
        }


        [Pure]
        public virtual string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName, string razorTemplateFilePath = null)
        {
            ITemplate instance;
            
            if (cacheName == null)
                instance = CreateTemplate(razorTemplate, null, model, razorTemplateFilePath);
            else
                instance = GetTemplate(razorTemplate, model, cacheName, razorTemplateFilePath);

            return Run(instance, viewBag);
        }


        [Pure]
        public virtual string Parse<T>(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName, string razorTemplateFilePath = null)
        {
            ITemplate instance;

            if (cacheName == null)
                instance = CreateTemplate(razorTemplate, typeof(T), model, razorTemplateFilePath);
            else
                instance = GetTemplate<T>(razorTemplate, model, cacheName, razorTemplateFilePath);

            return Run(instance, viewBag);
        }

        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> cacheNames, IEnumerable<string> razorTemplateFilePaths, bool parallel)
        {
            if (razorTemplates == null)
                throw new ArgumentException("Expected at least one entry in razorTemplates collection.");

            if ((models != null) && (razorTemplates.Count() != models.Count()))
                throw new ArgumentException("Expected same number of models as string templates to be processed.");

            if ((viewBags != null) && (razorTemplates.Count() != viewBags.Count()))
                throw new ArgumentException("Expected same number of viewBags as string templates to be processed.");

            if ((cacheNames != null) && (razorTemplates.Count() != cacheNames.Count()))
                throw new ArgumentException("Expected same number of cacheNames as string templates to be processed.");

            if ((razorTemplateFilePaths != null) && (razorTemplates.Count() != razorTemplateFilePaths.Count()))
                throw new ArgumentException("Expected same number of template file paths as string templates to be processed.");

            var modelList = (models == null) ? null : models.ToList();
            var viewBagList = (viewBags == null) ? null : viewBags.ToList();
            var cacheNameList = (cacheNames == null) ? null : cacheNames.ToList();
            var razorTemplateFilePathList = (razorTemplateFilePaths == null) ? null : razorTemplateFilePaths.ToList();

            //
            //  :Matt:
            //      What is up with the GetParallelQueryPlan() here?
            //          o   Everywhere else in the code we simply return the ParallelQueryPlan
            //              (this implements IEnumerablt<T>
            //          o   However, when I remove the ToArray() in the following code,
            //              the unit tests that call ParseMany() start failing, complaining
            //              about serialization (give it a try)
            //          o   Should all GetParallelQueryPlan() results call ToList()
            //              to force Linq to execute the results?
            //          o   If we do not call ToArray() below, then the Parse() 
            //              method never gets called.
            //          o   Something is wrong or inconsistent here.  :)
            //
            if (parallel)
            {
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t,
                        (modelList == null) ? null : modelList[i],
                        (viewBagList == null) ? null : viewBagList[i],
                        (cacheNameList == null) ? null : cacheNameList[i],
                        (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]))
                    .ToArray();
            }
            return razorTemplates.Select((t, i) => Parse(t,
                        (modelList == null) ? null : modelList[i],
                        (viewBagList == null) ? null : viewBagList[i],
                        (cacheNameList == null) ? null : cacheNameList[i],
                        (razorTemplateFilePathList == null) ? null : razorTemplateFilePathList[i]))
                    .ToArray();
        }

        public bool HasTemplate(string cacheName)
        {
            return _cache.ContainsKey(cacheName);
        }

        public bool RemoveTemplate(string cacheName)
        {
            CachedTemplateItem item;
            return _cache.TryRemove(cacheName, out item);
        }


        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The resolved template.</returns>
        public virtual ITemplate Resolve(string cacheName, object model)
        {
            CachedTemplateItem cachedItem;
            ITemplate instance = null;
            if (_cache.TryGetValue(cacheName, out cachedItem))
                instance = CreateTemplate(null, cachedItem.TemplateType, model, null);

            if (instance == null && _config.Resolver != null)
            {
                string template = _config.Resolver.Resolve(cacheName);
                if (!string.IsNullOrWhiteSpace(template))
                {
                    string filename = _config.Resolver.ResolveFilename(cacheName);
                    instance = GetTemplate(template, model, cacheName, filename);
                }
            }

            return instance;
        }

        public string Run(string cacheName, object model, DynamicViewBag viewBag)
        {
            if (string.IsNullOrWhiteSpace(cacheName))
                throw new ArgumentException("'cacheName' is a required parameter.");

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(cacheName, out item)))
                throw new InvalidOperationException("No template exists with name '" + cacheName + "'");

            ITemplate instance = CreateTemplate(null, item.TemplateType, model, null);

            return Run(instance, viewBag);
        }

        public string Run(ITemplate template, DynamicViewBag viewBag)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            return template.Run(CreateExecuteContext(viewBag));
        }

        /// <summary>
        /// Sets the model for the template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template instance.</param>
        /// <param name="model">The model instance.</param>
        private static void SetModel<T>(ITemplate template, T model)
        {
            if (model == null) return;

            var dynamicTemplate = template as ITemplate<dynamic>;
            if (dynamicTemplate != null)
            {
                dynamicTemplate.Model = model;
                return;
            }

            var staticModel = template as ITemplate<T>;
            if (staticModel != null)
            {
                staticModel.Model = model;
                return;
            }

            SetModelExplicit(template, model);
        }

        /// <summary>
        /// Sets the model for the template.
        /// </summary>
        /// <remarks>
        /// This method uses reflection to set the model property. As we can't guaruntee that we know
        /// what model type they will be using, we have to do the hard graft. The preference would be
        /// to use the generic <see cref="SetModel{T}"/> method instead.
        /// </remarks>
        /// <param name="template">The template instance.</param>
        /// <param name="model">The model instance.</param>
        private static void SetModelExplicit(ITemplate template, object model)
        {
            var type = template.GetType();
            var prop = type.GetProperty("Model");

            if (prop != null)
                prop.SetValue(template, model, null);
        }
        #endregion
    }
}