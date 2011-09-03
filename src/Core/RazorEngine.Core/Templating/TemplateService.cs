namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    using Compilation;
    using Parallel;
    using Text;

    /// <summary>
    /// Defines a template service.
    /// </summary>
    public class TemplateService : MarshalByRefObject, ITemplateService
    {
        #region Fields
        private readonly IActivator _activator;
        private readonly Func<ICompilerService> _compilerService;
        private readonly IEncodedStringFactory _stringFactory;

        private readonly ConcurrentDictionary<string, CachedTemplateItem> _cache = new ConcurrentDictionary<string, CachedTemplateItem>();
        private readonly ConcurrentBag<Assembly> _assemblies = new ConcurrentBag<Assembly>();
        
        private readonly ISet<string> _namespaces = new HashSet<string>();
        private readonly TypeLoader _loader;

        private bool disposed;
        private readonly object sync = new object();
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        /// <param name="stringFactory">The factory used to create encoded strings.</param>
        public TemplateService(Func<ICompilerService> compilerService, IActivator activator, IEncodedStringFactory stringFactory)
        {
            Contract.Requires(compilerService != null);

            _compilerService = compilerService;
            _activator = activator ?? new DefaultActivator();
            _stringFactory = stringFactory ?? new HtmlEncodedStringFactory();

            _namespaces.Add("System");
            _namespaces.Add("System.Collections.Generic");
            _namespaces.Add("System.Linq");

            _loader = new TypeLoader(AppDomain.CurrentDomain, _assemblies);
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        /// <param name="encoding">The string encoding to use.</param>
        public TemplateService(Func<ICompilerService> compilerService, IActivator activator, Encoding encoding)
            : this(compilerService, activator, GetEncodedStringFactory(encoding)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        /// <param name="stringFactory">The factory used to create encoded strings.</param>
        public TemplateService(Func<ICompilerService> compilerService, IEncodedStringFactory stringFactory)
            : this(compilerService, null, stringFactory) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        /// <param name="encoding">The string encoding to use.</param>
        public TemplateService(Func<ICompilerService> compilerService, Encoding encoding)
            : this(compilerService, null, GetEncodedStringFactory(encoding)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        public TemplateService(Func<ICompilerService> compilerService, IActivator activator)
            : this(compilerService, activator, null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="compilerService">The compiler service.</param>
        public TemplateService(Func<ICompilerService> compilerService)
            : this(compilerService, null, null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        public TemplateService()
            : this(Language.CSharp, null, null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        /// <param name="stringFactory">The factory used to create encoded strings.</param>
        public TemplateService(Language language, IActivator activator, IEncodedStringFactory stringFactory)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), null, stringFactory) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        /// <param name="encoding">The string encoding to use.</param>
        public TemplateService(Language language, IActivator activator, Encoding encoding)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), activator, GetEncodedStringFactory(encoding)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="stringFactory">The factory used to create encoded strings.</param>
        public TemplateService(Language language, IEncodedStringFactory stringFactory)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), null, stringFactory) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">The string encoding to use.</param>
        public TemplateService(Language language, Encoding encoding)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), GetEncodedStringFactory(encoding)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="activator">The activator used to create template instances.</param>
        public TemplateService(Language language, IActivator activator)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), activator, null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        public TemplateService(Language language)
            : this(() => CompilerServiceBuilder.GetCompilerService(language), null, null) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get { return _stringFactory; } }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        public void AddNamespace(string ns)
        {
            _namespaces.Add(ns);
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
            Contract.Requires(model != null);

            var modelType = model.GetType();
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
            var instance = _activator.CreateInstance(context);
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

            foreach (string ns in _namespaces)
                context.Namespaces.Add(ns);

            var result = _compilerService().CompileType(context);
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
            Contract.Requires(model != null);

            if (razorTemplate == null)
                throw new ArgumentNullException("razorTemplate");

            int hashCode = razorTemplate.GetHashCode();

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

            return instance.Run();
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

            return instance.Run();
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

            return instance.Run();
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

            return instance.Run();
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

            return instance.Run();
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

            return instance.Run();
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
                    .Select(t => Parse(t));

            return razorTemplates.Select(t => Parse(t));
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
                    .Select((t, i) => Parse(t, namesList[i]));

            return razorTemplates.Select((t, i) => Parse(t, namesList[i]));
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
            Contract.Requires(razorTemplate != null);
            Contract.Requires(models != null);

            // If we only have one model, leave early and process singular.
            if (models.Count() == 1)
                return new[] { Parse(razorTemplate, models.First()) };

            var modelType = models.First().GetType();
            var type = CreateTemplateType(razorTemplate, modelType);

            if (parallel)
                return GetParallelQueryPlan<object>()
                    .CreateQuery(models)
                    .Select(m => CreateTemplate(type, m).Run());

            return models.Select(m => CreateTemplate(type, m).Run());
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
                    .Select(m => CreateTemplate(type, m).Run());

            return models.Select(m => CreateTemplate(type, m).Run());
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
            Contract.Requires(razorTemplates != null);
            Contract.Requires(models != null);
            Contract.Requires(razorTemplates.Count() == models.Count(), "Expected same number of models as templates to be processed.");

            var modelList = models.ToList();

            if (parallel)
                return GetParallelQueryPlan<string>()
                    .CreateQuery(razorTemplates)
                    .Select((t, i) => Parse(t, modelList[i]));

            return razorTemplates.Select((t, i) => Parse(t, modelList[i]));
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
                    .Select((t, i) => Parse(t, modelList[i]));

            return razorTemplates.Select((t, i) => Parse(t, modelList[i]));
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
        public virtual IEnumerable<string> ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel)
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
                    .Select((t, i) => Parse(t, modelList[i], nameList[i]));

            return razorTemplates.Select((t, i) => Parse(t, modelList[i], nameList[i]));
        }

        /// <summary>
        /// Sets the model for the template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template instance.</param>
        /// <param name="model">The model instance.</param>
        private static void SetModel<T>(ITemplate template, T model)
        {
            var dynamicTemplate = template as ITemplate<dynamic>;
            if (dynamicTemplate != null)
            {
                dynamicTemplate.Model = model;
                return;
            }

            var staticModel = template as ITemplate<T>;
            if (staticModel != null)
                staticModel.Model = model;
        }

        /// <summary>
        /// Sets the model for the template.
        /// </summary>
        /// <param name="template">The template instance.</param>
        /// <param name="model">The model instance.</param>
        private static void SetModelExplicit(ITemplate template, object model)
        {
            var type = template.GetType();
            var prop = type.GetProperty("Model");

            prop.SetValue(template, model, null);
        }
        #endregion
    }
}