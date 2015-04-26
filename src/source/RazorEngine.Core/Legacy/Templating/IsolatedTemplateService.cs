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
    [Obsolete("Please use the IsolatedRazorEngine class instead.")]
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
        /// <param name="viewBag">This parameter is ignored, set the Viewbag with template.SetData(null, viewBag)</param>
        /// <returns>The instance of <see cref="ExecuteContext"/></returns>
        ExecuteContext ITemplateService.CreateExecuteContext(DynamicViewBag viewBag)
        {
            throw new NotSupportedException("This operation is not supported directly by the IsolatedTemplateService.");
        }

        /// <summary>
        /// Compiles the specified template.
        /// </summary>
        /// <param name="razorTemplate">The string template.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="name">The name of the template.</param>
        public void Compile(string razorTemplate, Type modelType, string name)
        {
            _proxy.Compile(razorTemplate, modelType, name);
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
        /// Returns whether or not a template by the specified name has been created already.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <returns>Whether or not the template has been created.</returns>
        public bool HasTemplate(string name)
        {
            return _proxy.HasTemplate(name);
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
        /// Resolves the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="model">The model for the template.</param>
        /// <returns>The resolved template.</returns>
        ITemplate ITemplateService.Resolve(string name, object model)
        {
            return _proxy.Resolve(name, model);
        }

        #endregion
        private void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("IsolatedTemplateService");
        }

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="templateType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ITemplate CreateTemplate(string razorTemplate, Type templateType, object model)
        {
            CheckDisposed();
            return _proxy.CreateTemplate(razorTemplate, templateType, model);
        }

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="templateTypes"></param>
        /// <param name="models"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> CreateTemplates(IEnumerable<string> razorTemplates, IEnumerable<Type> templateTypes, IEnumerable<object> models, bool parallel = false)
        {
            CheckDisposed();
            return _proxy.CreateTemplates(razorTemplates, templateTypes, models, parallel);
        }

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="model"></param>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public ITemplate GetTemplate(string razorTemplate, object model, string cacheName)
        {
            CheckDisposed();
            return _proxy.GetTemplate(razorTemplate, model, cacheName);
        }

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="models"></param>
        /// <param name="cacheNames"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        public IEnumerable<ITemplate> GetTemplates(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<string> cacheNames, bool parallel = false)
        {
            CheckDisposed();
            return _proxy.GetTemplates(razorTemplates, models, cacheNames, parallel);
        }

        /// <summary>
        /// Backwards Compat
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public string Parse(string razorTemplate, object model, DynamicViewBag viewBag, string cacheName)
        {
            CheckDisposed();
            if (model != null && 
                (CompilerServicesUtility.IsAnonymousType(model.GetType()) || CompilerServicesUtility.IsDynamicType(model.GetType())))
            {
                throw new ArgumentException("Anonymous types are not supported (use the new RazorEngineService/IsolatedRazorEngineService API)");
            }
            return _proxy.Parse(razorTemplate, model, viewBag, cacheName);
        }

        /// <summary>
        /// Backwards compat
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="models"></param>
        /// <param name="viewBags"></param>
        /// <param name="cacheNames"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        public IEnumerable<string> ParseMany(IEnumerable<string> razorTemplates, IEnumerable<object> models, IEnumerable<DynamicViewBag> viewBags, IEnumerable<string> cacheNames, bool parallel)
        {
            CheckDisposed();
            return _proxy.ParseMany(razorTemplates, models, viewBags, cacheNames, parallel);
        }

        /// <summary>
        /// Backwards compat
        /// </summary>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public string Run(string name, object model, DynamicViewBag viewBag)
        {
            CheckDisposed();
            return _proxy.Run(name, model, viewBag);
        }

        /// <summary>
        /// Backwards compat.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public string Run(ITemplate template, DynamicViewBag viewBag)
        {
            CheckDisposed();
            return _proxy.Run(template, viewBag);
        }

        /// <summary>
        /// Backwards compat
        /// </summary>
        /// <param name="razorTemplate"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public Type CreateTemplateType(string razorTemplate, Type modelType)
        {
            CheckDisposed();
            return _proxy.CreateTemplateType(razorTemplate, modelType);
        }

        /// <summary>
        /// Backwards compat.
        /// </summary>
        /// <param name="razorTemplates"></param>
        /// <param name="modelTypes"></param>
        /// <param name="parallel"></param>
        /// <returns></returns>
        public IEnumerable<Type> CreateTemplateTypes(IEnumerable<string> razorTemplates, IEnumerable<Type> modelTypes, bool parallel = false)
        {
            CheckDisposed();
            return _proxy.CreateTemplateTypes(razorTemplates, modelTypes, parallel);
        }
    }
}