namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Linq;

    using Compilation;
    using Configuration;
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
        /// Gets the template service configuration.
        /// </summary>
        ITemplateServiceConfiguration ITemplateService.Configuration { get { return null; } }

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
        /// Creates a new <see cref="ExecuteContext"/> used to tracking templates.
        /// </summary>
        /// <param name="viewBag">The view bag.</param>
        /// <returns>The instance of <see cref="ExecuteContext"/></returns>
        ExecuteContext ITemplateService.CreateExecuteContext(DynamicViewBag viewBag = null)
        {
            throw new NotSupportedException("This operation is not supported directly by the IsolatedTemplateService.");
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        public void Compile(string razorTemplate, Type modelType, string cacheName)
        {
            _proxy.Compile(razorTemplate, modelType, cacheName);
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
        /// Creates an instance of <see cref="ITemplate{T}"/> from the specified string template.
        /// </summary>
        /// <param name="razorTemplate">
        /// The string template.
        /// If templateType is not NULL, this parameter may be NULL (unused).
        /// </param>
        /// <param name="templateType">
        /// The template type or NULL if the template type should be dynamically created.
        /// If razorTemplate is not NULL, this parameter may be NULL (unused).
        /// </param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <returns>An instance of <see cref="ITemplate{T}"/>.</returns>
        public ITemplate CreateTemplate(string razorTemplate, Type templateType, object model)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (model != null)
            {
                if (CompilerServicesUtility.IsDynamicType(model.GetType()))
                    throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
            }

            return _proxy.CreateTemplate(razorTemplate, templateType, model);
        }

        /// <summary>
        /// Creates a set of templates from the specified string templates and models.
        /// </summary>
        /// <param name="razorTemplates">
        /// The set of templates to create or NULL if all template types are already created (see templateTypes).
        /// If this parameter is NULL, the the templateTypes parameter may not be NULL. 
        /// Individual elements in this set may be NULL if the corresponding templateTypes[i] is not NULL (precompiled template).
        /// </param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="templateTypes">
        /// The set of template types or NULL to dynamically create template types for each template.
        /// If this parameter is NULL, the the razorTemplates parameter may not be NULL. 
        /// Individual elements in this set may be NULL to dynamically create the template if the corresponding razorTemplates[i] is not NULL (dynamically compile template).
        /// </param>
        /// <param name="parallel">Flag to determine whether to create templates in parallel.</param>
        /// <returns>The enumerable set of template instances.</returns>
        public IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> templateTypes, IEnumerable<object> models, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (models != null)
            {
                foreach (object model in models)
                {
                    if (model != null)
                    {
                        if (CompilerServicesUtility.IsDynamicType(model.GetType()))
                            throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
                    }
                }
            }

            return _proxy.CreateTemplates(razorTemplates, templateTypes, models, parallel);
        }

        /// <summary>
        /// Creates a <see cref="Type"/> that can be used to instantiate an instance of a template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type or NULL if no model exists.</param>
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
        /// Creates a set of template types from the specfied string templates.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create <see cref="Type"/> instances for.</param>
        /// <param name="modelTypes">
        /// The set of model types or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="parallel">Flag to determine whether to create template types in parallel.</param>
        /// <returns>The set of <see cref="Type"/> instances.</returns>
        public IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> modelTypes, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (modelTypes != null)
            {
                foreach (Type modelType in modelTypes)
                {
                    if ((modelType != null) && CompilerServicesUtility.IsDynamicType(modelType))
                        throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
                }
            }

            return _proxy.CreateTemplateTypes(razorTemplates, modelTypes, parallel);
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
        /// <param name="model">The model or NULL if there is no model for this template.</param>
        /// <param name="cacheName">The name of the template type in the cache.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate GetTemplate(string razorTemplate, object model, string cacheName)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            return _proxy.GetTemplate(razorTemplate, model, cacheName);
        }

        /// <summary>
        /// Gets the set of template instances for the specified string templates. Cached templates will be considered
        /// and if they do not exist, new types will be created and instantiated.
        /// </summary>
        /// <param name="razorTemplates">The set of templates to create.</param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="cacheNames">The set of cache names.</param>
        /// <param name="parallel">Flag to determine whether to get the templates in parallel.</param>
        /// <returns>The set of <see cref="ITemplate"/> instances.</returns>
        public IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> cacheNames, bool parallel = false)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (models != null)
            {
                foreach (object model in models)
                {
                    if (model != null)
                    {
                        if (CompilerServicesUtility.IsDynamicType(model.GetType()))
                            throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
                    }
                }
            }

            return _proxy.GetTemplates(razorTemplates, models, cacheNames, parallel);
        }

        /// <summary>
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <returns>Whether or not the template has been created.</returns>
        public bool HasTemplate(string cacheName)
        {
            return _proxy.HasTemplate(cacheName);
        }

        /// <summary>
        /// Remove a template by the specified name from the cache.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <returns>Whether or not the template has been removed.</returns>
        public bool RemoveTemplate(string cacheName)
        {
            return _proxy.RemoveTemplate(cacheName);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (model != null)
            {
            if (CompilerServicesUtility.IsDynamicType(model.GetType()))
                throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
            }

           return _proxy.Parse(razorTemplate, model, viewBag, cacheName);
        }

        /// <summary>
        /// Parses and returns the result of the specified string template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="model">The model instance or NULL if no model exists.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <param name="cacheName">The name of the template type in the cache or NULL if no caching is desired.</param>
        /// <returns>The string result of the template.</returns>
        public string Parse<T>(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if (model != null)
            {
                if (CompilerServicesUtility.IsDynamicType(model.GetType()))
                    throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
            }

            return _proxy.Parse<T>(razorTemplate, model, viewBag, cacheName);
        }

        /// <summary>
        /// Parses the specified set of templates.
        /// </summary>
        /// <param name="razorTemplates">The set of string templates to parse.</param>
        /// <param name="models">
        /// The set of models or NULL if no models exist for all templates.
        /// Individual elements in this set may be NULL if no model exists for a specific template.
        /// </param>
        /// <param name="viewBags">
        /// The set of initial ViewBag contents or NULL for an initially empty ViewBag for all templates.
        /// Individual elements in this set may be NULL if an initially empty ViewBag is desired for a specific template.
        /// </param>
        /// <param name="cacheNames">
        /// The set of cache names or NULL if no caching is desired for all templates.
        /// Individual elements in this set may be NULL if caching is not desired for specific template.
        /// </param>
        /// <param name="parallel">Flag to determine whether parsing in templates.</param>
        /// <returns>The set of parsed template results.</returns>
        public IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> cacheNames, bool parallel)
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");

            if ((models != null) && (models.Count() != razorTemplates.Count()))
                throw new ArgumentException("Expected same number of models contents as templates to be processed.");

            if ((viewBags != null) && (viewBags.Count() != razorTemplates.Count()))
                throw new ArgumentException("Expected same number of ViewBag contents as templates to be processed.");

            if ((cacheNames != null) && (cacheNames.Count() != razorTemplates.Count()))
                throw new ArgumentException("Expected same number of cache names as templates to be processed.");

            if (models != null)
            {
                foreach (object model in models)
                {
                    if ((model != null) && CompilerServicesUtility.IsDynamicType(model.GetType()))
                        throw new ArgumentException("IsolatedTemplateService instances do not support anonymous or dynamic types.");
                }
            }

            return _proxy.ParseMany(razorTemplates, models, viewBags, cacheNames, parallel).ToList();
        }

        /// <summary>
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The resolved template.</returns>
        public ITemplate Resolve(string cacheName, object model)
        {
            return _proxy.Resolve(cacheName, model);
        }

        /// <summary>
        /// Runs the template with the specified cacheName.
        /// </summary>
        /// <param name="cacheName">The name of the template in cache.  The template must be in cache.</param>
        /// <param name="model">The model for the template or NULL if there is no model.</param>
        /// <param name="viewBag">The initial ViewBag contents NULL for an empty ViewBag.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(string cacheName, object model, DynamicViewBag viewBag)
        {
            return _proxy.Run(cacheName, model, viewBag);
        }

        /// <summary>
        /// Runs the template with the specified name.
        /// </summary>
        /// <param name="template">The template to run.</param>
        /// <param name="viewBag">The ViewBag contents or NULL for an initially empty ViewBag.</param>
        /// <returns>The string result of the template.</returns>
        public string Run(ITemplate template, DynamicViewBag viewBag)
        {
            return _proxy.Run(template, viewBag);
        }

        #endregion
    }
}