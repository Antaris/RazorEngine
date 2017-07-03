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
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;
    

    /// <summary>
    /// Provides template parsing and compilation in an isolated application domain.
    /// </summary>
    public class IsolatedRazorEngineService : IRazorEngineService
    {
        /// <summary>
        /// A helper interface to get a custom configuration into a new AppDomain.
        /// Classes inheriting this interface should be Serializable 
        /// (and not inherit from MarshalByRefObject).
        /// </summary>
        public interface IConfigCreator
        {
            /// <summary>
            /// Create a new configuration instance.
            /// This method should be executed in the new AppDomain.
            /// </summary>
            /// <returns></returns>
            ITemplateServiceConfiguration CreateConfiguration();
        }

        /// <summary>
        /// A simple <see cref="IConfigCreator"/> implementation to configure the <see cref="Language"/> and the <see cref="Encoding"/>.
        /// </summary>
        [Serializable]
        public class LanguageEncodingConfigCreator : IConfigCreator
        {
            private Language language;
            private Encoding encoding;

            /// <summary>
            /// Initializes a new <see cref="LanguageEncodingConfigCreator"/> instance
            /// </summary>
            /// <param name="language"></param>
            /// <param name="encoding"></param>
            public LanguageEncodingConfigCreator(Language language = Language.CSharp, Encoding encoding = Encoding.Html)
            {
                this.language = language;
                this.encoding = encoding;
            }

            /// <summary>
            /// Create the configuration.
            /// </summary>
            /// <returns></returns>
            public ITemplateServiceConfiguration CreateConfiguration()
            {
                return new TemplateServiceConfiguration()
                {
                    Language = language,
                    EncodedStringFactory = RazorEngineService.GetEncodedStringFactory(encoding)
                };
            }
        }

        /// <summary>
        /// A simple <see cref="IConfigCreator"/> implementation to use the default configuration.
        /// </summary>
        [Serializable]
        public class DefaultConfigCreator : IConfigCreator
        {
            /// <summary>
            /// Initializes a new <see cref="DefaultConfigCreator"/> instance
            /// </summary>
            public DefaultConfigCreator()
            {
            }

            /// <summary>
            /// Create the configuration.
            /// </summary>
            /// <returns></returns>
            public ITemplateServiceConfiguration CreateConfiguration()
            {
                // We need to use the DefaultCompilerServiceFactory because we
                // get conflicting TypeLoadExceptions in RazorEngineSourceReferenceResolver
                // that cannot be resolved.
                // ie) When not used inside the IsolatedRazorEngineService, we
                // need [SecuritySafeCritical] for the methods. However, inside
                // the Isolated service, it requires everything to be [SecurityCritical].
                return new TemplateServiceConfiguration()
                {
                    CompilerServiceFactory = new DefaultCompilerServiceFactory()
                };
            }
        }

        /// <summary>
        /// A simple sandbox helper to create the <see cref="IRazorEngineService"/>
        /// in the new <see cref="AppDomain"/>.
        /// </summary>
        public class SanboxHelper : CrossAppDomainObject
        {
            /// <summary>
            /// Create the <see cref="IRazorEngineService"/> in the new <see cref="AppDomain"/>.
            /// </summary>
            /// <param name="configCreator"></param>
            /// <returns></returns>
            public IRazorEngineService CreateEngine(IConfigCreator configCreator)
            {
                var config = configCreator.CreateConfiguration();
                return new RazorEngineService(config);
            }
        }

        #region Fields
        private readonly IRazorEngineService _proxy;
        private readonly AppDomain _appDomain;
        private bool _disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="IsolatedTemplateService"/>.
        /// </summary>
        /// <param name="configCreator">The instance to provide the configuration in the new <see cref="AppDomain"/>.</param>
        /// <param name="appDomainFactory">The application domain factory.</param>
        [SecurityCritical]
        internal IsolatedRazorEngineService(IConfigCreator configCreator, IAppDomainFactory appDomainFactory)
        {
            _appDomain = CreateAppDomain(appDomainFactory ?? new DefaultAppDomainFactory());
            var config = configCreator ?? new DefaultConfigCreator();
            
            ObjectHandle handle = 
                Activator.CreateInstanceFrom(
                    _appDomain, typeof(SanboxHelper).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(SanboxHelper).FullName
                );

            using (var helper = (SanboxHelper)handle.Unwrap())
            {
                _proxy = helper.CreateEngine(config);
            }
        }

        internal IRazorEngineService Proxy
        {
            get
            {
                return _proxy;
            }
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create()
        {
            return Create(null, (IAppDomainFactory)null);
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator config)
        {
            return Create(config, (IAppDomainFactory)null);
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create(IAppDomainFactory appDomainFactory)
        {
            return Create(null, appDomainFactory);
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create(Func<AppDomain> appDomainFactory)
        {
            return Create(null, new DelegateAppDomainFactory(appDomainFactory));
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator configCreator, Func<AppDomain> appDomainFactory)
        {
            return Create(configCreator, new DelegateAppDomainFactory(appDomainFactory));
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance which executes the templates in a new <see cref="AppDomain"/>.
        /// </summary>
        /// <returns></returns>
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator configCreator, IAppDomainFactory appDomainFactory)
        {
            configCreator = configCreator ?? new DefaultConfigCreator();
            var config = configCreator.CreateConfiguration();
            if (config.DisableTempFileLocking)
            {
                throw new InvalidOperationException("DisableTempFileLocking is not supported in the context of Isolation, because it will escape any kind of sandbox. Besides that it's not required because RazorEngine will be able to cleanup the tempfiles in this scenario. Just make sure to call AppDomain.Unload when you are done.");
            }

            var isolated = new IsolatedRazorEngineService(configCreator, appDomainFactory);
            return new DynamicWrapperService(isolated, true, config.AllowMissingPropertiesOnDynamic);
        }
        #endregion
        
        #region Methods

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
            if (disposing && !_disposed)
            {
                _proxy.Dispose();

                AppDomain.Unload(_appDomain);
                _disposed = true;
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
        /// Gets a given key from the <see cref="ITemplateManager"/> implementation.
        /// See <see cref="ITemplateManager.GetKey"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _proxy.GetKey(name, resolveType, context);
        }

        /// <summary>
        /// Checks if a given template is already cached.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            return _proxy.IsTemplateCached(key, modelType);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            _proxy.AddTemplate(key, templateSource);
        }

        /// <summary>
        /// Compiles the specified template and caches it.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="modelType">The model type.</param>
        public void Compile(ITemplateKey key, Type modelType = null)
        {
            _proxy.Compile(key, modelType);
        }

        /// <summary>
        /// Runs the given cached template.
        /// When the cache does not contain the template 
        /// it will be compiled and cached beforehand.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public void RunCompile(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _proxy.RunCompile(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// Runs the given cached template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public void Run(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _proxy.Run(key, writer, modelType, model, viewBag);
        }

        #endregion
    }
}