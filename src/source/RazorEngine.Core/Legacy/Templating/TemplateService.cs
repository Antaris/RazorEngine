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
            : this(new TemplateServiceConfiguration() { CompilerServiceFactory = new DefaultCompilerServiceFactory() }) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">the encoding.</param>
        internal TemplateService(Language language, Encoding encoding)
            : this(new TemplateServiceConfiguration() { Language = language, EncodedStringFactory = GetEncodedStringFactory(encoding), CompilerServiceFactory = new DefaultCompilerServiceFactory() }) { }
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
        /// Creates a new <see cref="InstanceContext"/> for creating template instances.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>An instance of <see cref="InstanceContext"/>.</returns>
        [Pure]
        protected virtual InstanceContext CreateInstanceContext(Type templateType)
        {
            return _service.Core.CreateInstanceContext(templateType);
        }

        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        public void AddNamespace(string ns)
        {
            Configuration.Namespaces.Add(ns);
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
            _service.Compile(GetKeyAndAdd(razorTemplate, name), CheckModelType(modelType));
        }

        /// <summary>
        /// Creates a ExecuteContext
        /// </summary>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public ExecuteContext CreateExecuteContext (DynamicViewBag viewBag = null) 
        {
            if (viewBag != null) 
                throw new NotSupportedException("This kind of usage is no longer supported, please switch to the non-obsolete API.");
            return _service.Core.CreateExecuteContext();
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="staticType">type used in the compilation.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        [Pure]
        public virtual ITemplate CreateTemplate(string razorTemplate, Type staticType, object model)
        {
            var key = GetKeyAndAdd(razorTemplate);
            ICompiledTemplate compiledTemplate;
            var check = CheckModel(model);
            Type modelType = check.Item2;
            model = check.Item1;
            
            if (staticType == null) {
                compiledTemplate = _service.CompileAndCacheInternal(key, modelType);
            }
            else
            {
                var source = _service.Core.Resolve(key);
                compiledTemplate = new CompiledTemplate(new CompilationData(null, null), key, source, staticType, modelType);
	        }
            return _service.Core.CreateTemplate(compiledTemplate, model, null);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="models">The set of models used to assign to templates.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <param name="types">the mode types.</param>
        /// <returns>The enumerable set of template instances.</returns>
        [Pure]
        public virtual IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> types,  IEnumerable<object> models, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);

            var l = razorTemplates.ToList();
            var typeList = (types ?? Enumerable.Repeat<Type>(null, l.Count)).ToList();
            var modelList = (models ?? Enumerable.Repeat<object>(null, l.Count)).ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => CreateTemplate(t, typeList[i], modelList[i]));

            return razorTemplates.Select((t, i) => CreateTemplate(t, typeList[i], modelList[i]));
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
            var result = _service.Core.CreateTemplateType(new LoadedTemplateSource(razorTemplate), CheckModelType(modelType));
            result.Item2.DeleteAll();
            return result.Item1;
        }

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="types">The modeltypes</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        [Pure]
        public virtual IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> types, bool parallel = false)
        {
            Contract.Requires(razorTemplates != null);
            var typeList = types.ToList();
            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => CreateTemplateType(t, typeList[i]));

            return razorTemplates.Select((t, i) => CreateTemplateType(t, typeList[i]));
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
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public virtual ITemplate GetTemplate(string razorTemplate, object model, string name)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");
            var key = GetKeyAndAdd(razorTemplate, name);

            var check = CheckModel(model);
            Type modelType = check.Item2;
            model = check.Item1;
            
            return _service.GetTemplate(key, modelType, model, null);
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
        public virtual ITemplate GetTemplate<T>(string razorTemplate, object model, string name)
        {
            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");
            
            var key = GetKeyAndAdd(razorTemplate, name);
            var check = CheckModel(model);
            model = check.Item1;
            return _service.GetTemplate(key, typeof(T), model, null);
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

            var l = razorTemplates.ToList();
            var modelList = (models ?? Enumerable.Repeat<object>(null, l.Count)).ToList();
            var nameList = (names ?? Enumerable.Repeat<string>(null, l.Count)).ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(l)
                    .Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));

            return l.Select((t, i) => GetTemplate(t, modelList[i], nameList[i]));
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="viewBag">The viewbag.</param>
        /// <param name="cacheName">The cacheName.</param>
        /// <returns>The string result of the template.</returns>
        [Pure]
        public virtual string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName)
        {
            ITemplate instance;

            if (cacheName == null)
                instance = CreateTemplate(razorTemplate, null, model);
            else
                instance = GetTemplate(razorTemplate, model, cacheName);

            return Run(instance, viewBag);
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
        /// <param name="models">The set of models.</param>
        /// <param name="viewBags">The viewbags</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        public virtual IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> names, bool parallel)
        {
            Contract.Requires(razorTemplates != null);
            var templates = razorTemplates.ToList();

            var modelList = (models ?? Enumerable.Repeat<object>(null, templates.Count)).ToList();
            var bags = (viewBags ?? Enumerable.Repeat<DynamicViewBag>(null, templates.Count)).ToList();
            var nameList = (names ?? Enumerable.Repeat<string>(null, templates.Count)).ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(templates)
                    .Select((t, i) => Parse(t, modelList[i], bags[i], nameList[i]))
                    .ToArray();

            return templates.Select((t, i) => Parse(t, modelList[i], bags[i], nameList[i])).ToArray();
        }

        /// <summary>
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>Whether or not the template has been created.</returns>
        public bool HasTemplate(string name)
        {
            throw new InvalidOperationException("This member is no longer supported!");
        }
        
        /// <summary>
        /// NOT SUPPORTED.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveTemplate(string name) 
        {
            throw new InvalidOperationException("This member is no longer supported!");
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string name, object model)
        {
            var check = CheckModel(model);
            var modelType = check.Item2;
            model = check.Item1;
            return _service.GetTemplate(
                _service.GetKey(name), modelType, model, null);
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model.</param>
        /// <param name="viewBag">The viewBag.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(string name, object model, DynamicViewBag viewBag)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("'name' is a required parameter.");
            
            using(var writer = new System.IO.StringWriter())
            {
                var check = CheckModel(model);
                var modelType = check.Item2;
                model = check.Item1;
                _service.Run(_service.GetKey(name), writer, modelType, model, viewBag);
                return writer.ToString();
	        }
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="viewBag">The viewbag.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(ITemplate template, DynamicViewBag viewBag)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            using (var writer = new System.IO.StringWriter())
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