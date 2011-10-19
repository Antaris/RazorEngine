namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Linq;

    using Compilation;
    using Text;

    /// <summary>
    /// Provides template parsing and compilation in an isolated application domain.
    /// </summary>
    public class IsolatedTemplateService : ITemplateService
    {
        #region Fields
        private static readonly Type TemplateServiceType = typeof(TemplateService);
        private readonly ITemplateService _proxy;
        private readonly AppDomain _appDomain;
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        public IsolatedTemplateService()
            : this(Language.CSharp, Encoding.Html, (IAppDomainFactory)null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        public IsolatedTemplateService(Language language)
            : this(language, Encoding.Html, (IAppDomainFactory)null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public IsolatedTemplateService(Encoding encoding)
            : this(Language.CSharp, encoding, (IAppDomainFactory)null) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        /// <param name="appDomainFactory">The application domain factory.</param>
        public IsolatedTemplateService(IAppDomainFactory appDomainFactory)
            : this(Language.CSharp, Encoding.Html, appDomainFactory) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>.
        /// </summary>
        /// <param name="appDomainFactory">The delegate used to create an application domain.</param>
        public IsolatedTemplateService(Func<AppDomain> appDomainFactory) 
            : this(Language.CSharp, Encoding.Html, appDomainFactory) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="appDomainFactory">The application domain factory.</param>
        public IsolatedTemplateService(Language language, Encoding encoding, IAppDomainFactory appDomainFactory)
        {
            _appDomain = CreateAppDomain(appDomainFactory ?? new DefaultAppDomainFactory());

            string assemblyName = TemplateServiceType.Assembly.FullName;
            string typeName = TemplateServiceType.FullName;

            _proxy = (ITemplateService)_appDomain.CreateInstance(
                assemblyName, typeName, false, BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] { language, encoding }, CultureInfo.CurrentCulture, null).Unwrap();
        }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="appDomainFactory">The delegate used to create an application domain.</param>
        public IsolatedTemplateService(Language language, Func<AppDomain> appDomainFactory)
            : this(language, Encoding.Html, new DelegateAppDomainFactory(appDomainFactory)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="appDomainFactory">The delegate used to create an application domain.</param>
        public IsolatedTemplateService(Language language, Encoding encoding, Func<AppDomain> appDomainFactory)
            : this(language, encoding, new DelegateAppDomainFactory(appDomainFactory)) { }

        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="appDomainFactory">The delegate used to create an application domain.</param>
        public IsolatedTemplateService(Encoding encoding, Func<AppDomain> appDomainFactory)
            : this(Language.CSharp, encoding, new DelegateAppDomainFactory(appDomainFactory)) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        IEncodedStringFactory ITemplateService.EncodedStringFactory { get { return null; } }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a namespace that will be imported into the template.
        /// </summary>
        /// <param name="ns">The namespace to be imported.</param>
        public void AddNamespace(string ns)
        {
            _proxy.AddNamespace(ns);
        }

        /// <summary>
        /// Creates an application domain.
        /// </summary>
        /// <param name="factory">The application domain factory.</param>
        /// <returns>An instance of <see cref="AppDomain"/>.</returns>
        private static AppDomain CreateAppDomain(IAppDomainFactory factory)
        {
            var domain = factory.CreateAppDomain();
            if (domain == null)
                throw new InvalidOperationException("The application domain factory did not create an application domain.");

            if (domain == AppDomain.CurrentDomain)
                throw new InvalidOperationException("The application domain factory returned the current application domain which cannot be used for isolation.");

            return domain;
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate CreateTemplate(string razorTemplate)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.CreateTemplate(razorTemplate);
        }

        /// <summary>
        /// Creates an instance of <see cref="ITemplate{T}"/> from the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public ITemplate CreateTemplate<T>(string razorTemplate, T model)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.CreateTemplate(razorTemplate, model);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        public IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.CreateTemplates(razorTemplates, parallel);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of templates to create <see cref="ITemplate"/> instances for.</param>
        /// <param name="models">The set of models used to assign to templates.</param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        public IEnumerable<ITemplate> CreateTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.CreateTemplates(razorTemplates, models, parallel);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public Type CreateTemplateType(string razorTemplate)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.CreateTemplateType(razorTemplate);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>An instance of <see cref="Type"/>.</returns>
        public Type CreateTemplateType(string razorTemplate, Type modelType)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(modelType))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.CreateTemplateType(razorTemplate, modelType);
        }

        /// <summary>
        /// Crates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        public IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.CreateTemplateTypes(razorTemplates, parallel);
        }

        /// <summary>
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        public IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, Type modelType, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (modelType != null && CompilerServicesUtility.IsDynamicType(modelType))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.CreateTemplateTypes(razorTemplates, modelType, parallel);
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        /// <remarks>
        /// This method ensures the AppDomain is unloaded and any template assemblies are unloaded with it.
        /// </remarks>
        /// <param name="disposing">Flag to determine whether the instance is being disposed explicitly.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                _proxy.Dispose();

                AppDomain.Unload(_appDomain);
                disposed = true;
            }
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets an instance of the template using the cached compiled type, or compiles the template type
        /// if it does not exist in the cache.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate GetTemplate(string razorTemplate, string name)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.GetTemplate(razorTemplate, name);
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
        public ITemplate GetTemplate<T>(string razorTemplate, T model, string name)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.GetTemplate(razorTemplate, model, name);
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        public IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.GetTemplates(razorTemplates, names, parallel);
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
        public IEnumerable<ITemplate> GetTemplates<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.GetTemplates(razorTemplates, models, names, parallel);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse(string razorTemplate)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.Parse(razorTemplate);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template. 
        /// This method will provide a cache check to see if the compiled template type already exists and is valid.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="name">The name of the cached template type.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse(string razorTemplate, string name)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.Parse(razorTemplate, name);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse<T>(string razorTemplate, T model)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.Parse(razorTemplate, model);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance.</param>
        /// <param name="name">The name of the template type in the cache.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse<T>(string razorTemplate, T model, string name)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.Parse(razorTemplate, model, name);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ITemplateService.ParseMany(IEnumerable<string> razorTemplates, bool parallel)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.ParseMany(razorTemplates, parallel);
        }

        /// <summary>
        /// Parses the specified array of templates.
        /// </summary>
        /// <param name="razorTemplates">The array of string templates to partse.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The array of parsed template results.</returns>
        public string[] ParseMany(string[] razorTemplates, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.ParseMany(razorTemplates, parallel).ToArray();
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ITemplateService.ParseMany(IEnumerable<string> razorTemplates, IEnumerable<string> names, bool parallel)
        {
            return ParseMany(razorTemplates.ToArray(), names.ToArray(), parallel);
        }

        /// <summary>
        /// Parses the specified array of templates.
        /// </summary>
        /// <param name="razorTemplates">The array of string templates to partse.</param>
        /// <param name="names">The array of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The array of parsed template results.</returns>
        public string[] ParseMany(string[] razorTemplates, string[] names, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.ParseMany(razorTemplates, names, parallel).ToArray();
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ITemplateService.ParseMany<T>(string razorTemplate, IEnumerable<T> models, bool parallel)
        {
            return ParseMany(razorTemplate, models.ToArray(), parallel);
        }

        /// <summary>
        /// Parses the template and merges with the many models provided.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplate">The razor template.</param>
        /// <param name="models">The array of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in parallel.</param>
        /// <returns>The array of parsed template results.</returns>
        public string[] ParseMany<T>(string razorTemplate, T[] models, bool parallel = false)
        {
            return _proxy.ParseMany(razorTemplate, models, parallel).ToArray();
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ITemplateService.ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, bool parallel)
        {
            return ParseMany(razorTemplates.ToArray(), models.ToArray(), parallel);
        }

        /// <summary>
        /// Parses the specified array of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The array of string templates to partse.</param>
        /// <param name="models">The array of models.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The array of parsed template results.</returns>
        public string[] ParseMany<T>(string[] razorTemplates, T[] models, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.ParseMany(razorTemplates, models, parallel).ToArray();
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The set of string templates to partse.</param>
        /// <param name="models">The set of models.</param>
        /// <param name="names">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        IEnumerable<string> ITemplateService.ParseMany<T>(IEnumerable<string> razorTemplates, IEnumerable<T> models, IEnumerable<string> names, bool parallel)
        {
            return ParseMany(razorTemplates.ToArray(), models.ToArray(), names.ToArray(), parallel);
        }

        /// <summary>
        /// Parses the specified array of templates.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="razorTemplates">The array of string templates to partse.</param>
        /// <param name="models">The array of models.</param>
        /// <param name="names">The array of cache names.</param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The array of parsed template results.</returns>
        public string[] ParseMany<T>(string[] razorTemplates, T[] models, string[] names, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (CompilerServicesUtility.IsDynamicType(typeof(T)))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");

            return _proxy.ParseMany(razorTemplates, models, names, parallel).ToArray(); 
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate ITemplateService.Resolve(string name)
        {
            return _proxy.Resolve(name);
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate ITemplateService.Resolve(string name, object model)
        {
            return _proxy.Resolve(name, model);
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate ITemplateService.Resolve<T>(string name, T model)
        {
            return _proxy.Resolve(name, model);
        }
        #endregion
    }
}