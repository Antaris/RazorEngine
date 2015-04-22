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
    using System.Threading.Tasks;
#if RAZOR4
    using System.Runtime.ExceptionServices;
#endif

    /// <summary>
    /// Defines a template service.
    /// </summary>
    [Obsolete("Use RazorEngineService instead")]
    public class TemplateService : CrossAppDomainObject, ITemplateService
    {
        #region Fields
        private readonly RazorEngineService _service;
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
            _service = new RazorEngineService(config);
        }

        internal TemplateService(RazorEngineService service)
        {
            Contract.Requires(service != null);
            _service = service;
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
        public ITemplateServiceConfiguration Configuration { get { return _service.Configuration; } }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get { return _service.Configuration.EncodedStringFactory; } }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        public void AddNamespace(string ns)
        {
            Configuration.Namespaces.Add(ns);
        }
        private ITemplateKey GetKeyAndAdd(string template, string name = null)
        {
            if (name == null)
            {
                name = "dynamic_" + Guid.NewGuid().ToString();
            }
            var key = _service.GetKey(name);
            var source = new LoadedTemplateSource(template);
            _service.Configuration.TemplateManager.AddDynamic(key, source);
            return key;
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template.</param>
        public void Compile(string razorTemplate, string name)
        {
            Compile(razorTemplate, null, name);
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="name">The name of the template.</param>
        public void Compile(string razorTemplate, Type modelType, string name)
        {
            Contract.Requires(razorTemplate != null);
            Contract.Requires(name != null);

            int hashCode = razorTemplate.GetHashCode();

            Type type = CreateTemplateType(razorTemplate, modelType);
            var item = new CachedTemplateItem(hashCode, type);

            _cache.AddOrUpdate(name, item, (n, i) => item);
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template.</param>
        public void Compile<T>(string razorTemplate, string name)
        {
            Compile(razorTemplate, typeof(T), name);
        }

        /// <summary>
        /// Creates a new <see cref="InstanceContext"/> for creating template instances.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>An instance of <see cref="InstanceContext"/>.</returns>
        [Pure]
        protected virtual InstanceContext CreateInstanceContext(Type templateType)
        {
            return _service.Core.CreateInstanceContext(templateType);
        }

        private Type CheckModelType(Type modelType)
        {
            if (modelType == null)
            {
                return null;
            }
            if (CompilerServicesUtility.IsAnonymousTypeRecursive(modelType))
            {
                //throw new ArgumentException("Cannot use anonymous type as model type.");
                modelType = null;
            }
            if (modelType != null && CompilerServicesUtility.IsDynamicType(modelType))
            {
                modelType = null;
            }
            return modelType;
        }

        private Tuple<object, Type> CheckModel(object model)
        {
            if (model == null)
            {
                return Tuple.Create((object)null, (Type)null);
            }
            Type modelType = (model == null) ? typeof(object) : model.GetType();

            bool isAnon = CompilerServicesUtility.IsAnonymousTypeRecursive(modelType);
            if (isAnon ||
                CompilerServicesUtility.IsDynamicType(modelType))
            {
                modelType = null;
                if (isAnon || Configuration.AllowMissingPropertiesOnDynamic)
                {
                    model = RazorDynamicObject.Create(model, Configuration.AllowMissingPropertiesOnDynamic);
                }
            }
            return Tuple.Create(model, modelType);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public virtual ITemplate CreateTemplate(string razorTemplate)
        {
            var type = CreateTemplateType(razorTemplate);

            var instance = CreateTemplate(type);
            return instance;
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public virtual ITemplate CreateTemplate(string razorTemplate, object model)
        {
            return CreateTemplate(razorTemplate, _objectType, model);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The expected model type.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public virtual ITemplate CreateTemplate(string razorTemplate, Type modelType, object model)
        {
            var type = CreateTemplateType(razorTemplate, modelType);

            var instance = CreateTemplate(type, model);
            return instance;
        }

        /// <summary>
        /// Creates an instance of an <see cref="ITemplate"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>The <see cref="ITemplate"/> instance.</returns>
        [Pure]
        protected virtual ITemplate CreateTemplate(Type templateType)
        {
            var context = CreateInstanceContext(templateType);
            var instance = _config.Activator.CreateInstance(context);
            instance.TemplateService = this;

            return instance;
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The <see cref="ITemplate"/> instance.</returns>
        [Pure]
        protected virtual ITemplate CreateTemplate(Type templateType, object model)
        {
            var instance = CreateTemplate(templateType);
            SetModelExplicit(instance, model);

            return instance;
        }

        /// <summary>
        /// Creates an instance of an <see cref="ITemplate"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="templateType">The template type.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The <see cref="ITemplate"/> instance.</returns>
        [Pure]
        protected virtual ITemplate CreateTemplate<T>(Type templateType, T model)
        {
            var instance = CreateTemplate(templateType);
            SetModel(instance, model);

            return instance;
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate{T}"/> from the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        [Pure]
        public virtual ITemplate CreateTemplate<T>(string razorTemplate, T model)
        {
            var type = CreateTemplateType(razorTemplate, typeof(T));

            var instance = CreateTemplate(type, model);
            return instance;
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [Pure]
        public virtual IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select(t => CreateTemplate(t));

            return razorTemplates.Select(t => CreateTemplate(t));
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="models">The set of models used to assign to templates.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [Pure]
        public virtual IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(models != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected the same number of model instances as the number of templates to be processed.");

            var modelList = models.ToList();
            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => CreateTemplate(t, modelList[i]));

            return razorTemplates.Select((t, i) => CreateTemplate(t, modelList[i]));
                    }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="models">The set of models used to assign to templates.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [Pure]
        public virtual IEnumerable<ITemplate> CreateTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel = false)
                    {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(models != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected the same number of model instances as the number of templates to be processed.");

            var modelList = models.ToList();

            if (parallel)
                    return GetParallelQueryPlan<string>()
                        .CreateQuery(razorTemplates)
                    .Select((t, i) => CreateTemplate(t, modelList[i]));

            return razorTemplates.Select((t, i) => CreateTemplate(t, modelList[i]));
            }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        [Pure]
        public virtual Type CreateTemplateType(string razorTemplate)
        {
            return CreateTemplateType(razorTemplate, null);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        [Pure]
        public virtual Type CreateTemplateType(string razorTemplate, Type modelType)
        {
            var context = new TypeContext
                              {
                                  ModelType = modelType,
                                  TemplateContent = razorTemplate,
                                  TemplateType = (modelType == null) ? typeof(TemplateBase) : typeof(TemplateBase<>)
                              };

            if (_config.BaseTemplateType != null)
                context.TemplateType = _config.BaseTemplateType;

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

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        [Pure]
        public virtual IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select(CreateTemplateType);

            return razorTemplates.Select(CreateTemplateType);
        }

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        [Pure]
        public virtual IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, Type modelType, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(modelType != null);

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select(t => CreateTemplateType(t, modelType));

            return razorTemplates.Select(t => CreateTemplateType(t, modelType));
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing">Are we explicitly disposing of this instance?</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                _service.Dispose();
                disposed = true;
            }
            base.Dispose(disposing);
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

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public virtual ITemplate GetTemplate(string razorTemplate, string name)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");

            int hashCode = razorTemplate.GetHashCode();

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item) && item.CachedHashCode == hashCode))
            {
                Type type = CreateTemplateType(razorTemplate);
                item = new CachedTemplateItem(hashCode, type);

                _cache.AddOrUpdate(name, item, (n, i) => item);
            }

            return CreateTemplate(item.TemplateType);
        }

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public virtual ITemplate GetTemplate(string razorTemplate, object model, string name)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");
            var key = GetKeyAndAdd(razorTemplate, cacheName);

            var check = CheckModel(model);
            Type modelType = check.Item2;
            model = check.Item1;

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item) && item.CachedHashCode == hashCode))
            {
                Type type = CreateTemplateType(razorTemplate, model.GetType());
                item = new CachedTemplateItem(hashCode, type);

                _cache.AddOrUpdate(name, item, (n, i) => item);
            }

            var instance = CreateTemplate(item.TemplateType, model);
            return instance;
        }

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public virtual ITemplate GetTemplate<T>(string razorTemplate, T model, string name)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");

            int hashCode = razorTemplate.GetHashCode();

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item) && item.CachedHashCode == hashCode))
            {
                Type type = CreateTemplateType(razorTemplate, typeof(T));
                item = new CachedTemplateItem(hashCode, type);

                _cache.AddOrUpdate(name, item, (n, i) => item);
            }

            var instance = CreateTemplate(item.TemplateType, model);
            return instance;
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        public virtual IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(names != null);
            Contract.Requires(razorTemplates.Count() == names.Count(),
                              "Expected same number of cache names as string templates to be processed.");

            var nameList = names.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => GetTemplate(t, nameList[i]));

            return razorTemplates.Select((t, i) => GetTemplate(t, nameList[i]));
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        public virtual IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> names, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(names != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected same number of models as string templates to be processed.");
            Contract.Requires(razorTemplates.Count() == names.Count(),
                              "Expected same number of cache names as string templates to be processed.");

            var modelList = models.ToList();
            var nameList = names.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));

            return razorTemplates.Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        public virtual IEnumerable<ITemplate> GetTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(names != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected same number of models as string templates to be processed.");
            Contract.Requires(razorTemplates.Count() == names.Count(),
                              "Expected same number of cache names as string templates to be processed.");

            var modelList = models.ToList();
            var nameList = names.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));

            return razorTemplates.Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse(string razorTemplate)
        {
            var instance = CreateTemplate(razorTemplate);
            return Run(instance);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template. 
        /// This method will look for a cached version of the template before generating a new template.
        /// </summary>
        /// <param name="razorTemplate">The template to parse.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public virtual string Parse(string razorTemplate, string name)
        {
            var instance = GetTemplate(razorTemplate, name);
            return Run(instance);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse(string razorTemplate, object model)
        {
            var instance = CreateTemplate(razorTemplate, model);
            return Run(instance);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The expected model type.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse(string razorTemplate, Type modelType, object model)
        {
            var instance = CreateTemplate(razorTemplate, modelType, model);
            return Run(instance);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse<T>(string razorTemplate, T model)
        {
            var instance = CreateTemplate(razorTemplate, model);
            return Run(instance);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public virtual string Parse(string razorTemplate, object model, string name)
        {
            var instance = GetTemplate(razorTemplate, model, name);
            return Run(instance);
        }


        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public virtual string Parse<T>(string razorTemplate, T model, string name)
        {
            var instance = GetTemplate(razorTemplate, model, name);
            return Run(instance);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select(t => Parse(t))
                    .ToArray();

            return razorTemplates.Select(t => Parse(t)).ToArray();
        }


        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse<T>(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName)
        {
            using(var writer = new System.IO.StringWriter())
            {
                var key = GetKeyAndAdd(razorTemplate, cacheName);
                var check = CheckModel(model);
                model = check.Item1;
                _service.RunCompile(GetKeyAndAdd(razorTemplate, cacheName), writer, typeof(T), model, viewBag);
                return writer.ToString();
	        }
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(names != null);
            Contract.Requires(razorTemplates.Count() == names.Count(), "Expected same number of cache names as templates to be processed.");

            var namesList = names.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t, namesList[i]))
                    .ToArray();

            return razorTemplates.Select((t, i) => Parse(t, namesList[i])).ToArray();
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <remarks>
        /// This method makes assumptions that all models are of the same type, or have a common base type. This is done
        /// by using the first model's GetType() method to resolve what type it should be. Feels dirty.
        /// </remarks>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany(string razorTemplate, IEnumerable<object> models, bool parallel = false)
        {
            return ParseMany(razorTemplate, _objectType, models, parallel);
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <remarks>
        /// This method makes assumptions that all models are of the same type, or have a common base type. This is done
        /// by using the first model's GetType() method to resolve what type it should be. Feels dirty.
        /// </remarks>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="modelType">The expected model type.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany(string razorTemplate, Type modelType, IEnumerable<object> models, bool parallel = false)
        {
            Contract.Requires(razorTemplate != null);
            Contract.Requires(modelType != null);
            Contract.Requires(models != null);

            // If we only have one model, leave early and process singular.
            if (models.Count() == 1)
                return new[] { Parse(razorTemplate, modelType, models.First()) };

            // Compile the type just the once, and reuse that for each model.
            var type = CreateTemplateType(razorTemplate, modelType);

            if (parallel)
                return GetParallelQueryPlan<object>()
                    .CreateQuery(models)
                    .Select(m => Run(CreateTemplate(type, m)))
                    .ToArray();

            return models.Select(m => Run(CreateTemplate(type, m))).ToArray(); 
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany<T>(string razorTemplate, IEnumerable<T> models, bool parallel = false)
            {
            Contract.Requires(razorTemplate != null);
            Contract.Requires(models != null);

            var type = CreateTemplateType(razorTemplate, typeof(T));

            if (parallel)
                return GetParallelQueryPlan<T>()
                    .CreateQuery(models)
                    .Select(m => Run(CreateTemplate(type, m)))
                    .ToArray();

            return models.Select(m => Run(CreateTemplate(type, m))).ToArray();
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, bool parallel = false)
        {
            return ParseMany(razorTemplates, _objectType, models, parallel);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="modelType">The expected model type.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, Type modelType, IEnumerable<object> models, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(modelType != null);
            Contract.Requires(models != null);
            Contract.Requires(razorTemplates.Count() == models.Count(), "Expected same number of models as templates to be processed.");

            var modelList = models.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t, modelType, modelList[i]))
                    .ToArray();

            return razorTemplates.Select((t, i) => Parse(t, modelList[i])).ToArray(); 
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        [Pure]
        public virtual IEnumerable<string> ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(models != null);
            Contract.Requires(razorTemplates.Count() == models.Count(), "Expected same number of models as templates to be processed.");

            var modelList = models.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t, modelList[i]))
                    .ToArray();

            return razorTemplates.Select((t, i) => Parse(t, modelList[i])).ToArray();
            }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        public virtual IEnumerable<string> ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            Contract.Requires(names != null);
            Contract.Requires(razorTemplates.Count() == models.Count(),
                              "Expected same number of models as string templates to be processed.");
            Contract.Requires(razorTemplates.Count() == names.Count(),
                              "Expected same number of cache names as string templates to be processed.");

            var modelList = models.ToList();
            var nameList = names.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t, modelList[i], nameList[i]))
                    .ToArray();

            return razorTemplates.Select((t, i) => Parse(t, modelList[i], nameList[i])).ToArray();
        }

        /// <summary>
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>Whether or not the template has been created.</returns>
        public bool HasTemplate(string name)
        {
            return _cache.ContainsKey(name);
        }
        
        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string name)
        {
            CachedTemplateItem cachedItem;
            ITemplate instance = null;
            if (_cache.TryGetValue(name, out cachedItem))
                instance = CreateTemplate(cachedItem.TemplateType);

            if (instance == null && _config.Resolver != null)
            {
                string template = _config.Resolver.Resolve(name);
                if (!string.IsNullOrWhiteSpace(template))
                    instance = GetTemplate(template, name);
            }

            return instance;
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string name, object model)
        {
            CachedTemplateItem cachedItem;
            ITemplate instance = null;
            if (_cache.TryGetValue(name, out cachedItem))
                instance = CreateTemplate(cachedItem.TemplateType, model);

            if (instance == null && _config.Resolver != null)
        {
                string template = _config.Resolver.Resolve(name);
                if (!string.IsNullOrWhiteSpace(template))
                    instance = GetTemplate(template, model, name);
            }

            return instance;
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve<T>(string name, T model)
        {
            CachedTemplateItem cachedItem;
            ITemplate instance = null;
            if (_cache.TryGetValue(name, out cachedItem))
                instance = CreateTemplate(cachedItem.TemplateType, model);

            if (instance == null && _config.Resolver != null)
            {
                string template = _config.Resolver.Resolve(name);
                if (!string.IsNullOrWhiteSpace(template))
                    instance = GetTemplate(template, model, name);
            }

            return instance;
        }

        /// <summary>
        /// Runs the specified template and returns the result.
        /// </summary>
        /// <param name="template">The template to run.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(ITemplate template)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            return template.Run(new ExecuteContext());
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("'name' is a required parameter.");

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item)))
                throw new InvalidOperationException("No template exists with name '" + name + "'");

            var template = CreateTemplate(item.TemplateType);
            return template.Run(new ExecuteContext());
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(string name, object model)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("'name' is a required parameter.");

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item)))
                throw new InvalidOperationException("No template exists with name '" + name + "'");

            var template = CreateTemplate(item.TemplateType, model);
            return template.Run(new ExecuteContext());
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <returns>The string result of the template.</returns>
        public string Run<T>(string name, T model)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("'name' is a required parameter.");

            CachedTemplateItem item;
            if (!(_cache.TryGetValue(name, out item)))
                throw new InvalidOperationException("No template exists with name '" + name + "'");

            var template = CreateTemplate(item.TemplateType, model);
            return template.Run(new ExecuteContext());
        }

        /// <summary>
        /// Sets the model for the template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template instance.</param>
        /// <param name="model">The model instance.</param>
        private static void SetModel<T>(ITemplate template, T model)
            {
                template.SetData(null, viewBag);
#if RAZOR4
                try
                {
                    template.Run(CreateExecuteContext(), writer).Wait();
                }
                catch (AggregateException ex)
                {
                    ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
                }
#else
                template.Run(CreateExecuteContext(), writer);
#endif
                return writer.ToString();
            }
        }

        #endregion
    }
}