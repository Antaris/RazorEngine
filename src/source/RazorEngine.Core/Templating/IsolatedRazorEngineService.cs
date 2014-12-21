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
    //[SecuritySafeCritical]
    public class IsolatedRazorEngineService : IRazorEngineService
    {
        public interface IConfigCreator
        {
            ITemplateServiceConfiguration CreateConfiguration();
        }
        [Serializable]
        public class LanguageEncodingConfigCreator : IConfigCreator
        {
            Language language; 
            Encoding encoding;
            public LanguageEncodingConfigCreator(Language language = Language.CSharp, Encoding encoding = Encoding.Html)
            {
                this.language = language;
                this.encoding = encoding;
            }
            public ITemplateServiceConfiguration CreateConfiguration()
            {
                return new TemplateServiceConfiguration()
                {
                    Language = language,
                    EncodedStringFactory = RazorEngineService.GetEncodedStringFactory(encoding)
                };
            }
        }

        [Serializable]
        public class DefaultConfigCreator : IConfigCreator
        {
            public DefaultConfigCreator()
            {
            }
            public ITemplateServiceConfiguration CreateConfiguration()
            {
                return new TemplateServiceConfiguration();
            }
        }

        public class SanboxHelper : MarshalByRefObject
        {
            //[SecurityCritical]
            public IRazorEngineService CreateEngine(IConfigCreator configCreator)
            {
                //(new PermissionSet(PermissionState.Unrestricted)).Assert();
                var config = configCreator.CreateConfiguration();
                return new RazorEngineService(config);
            }
        }

        #region Fields
        private static readonly Type RazorEngineServiceType = typeof(RazorEngineService);
        private readonly IRazorEngineService _proxy;
        private readonly AppDomain _appDomain;
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="IsolatedTemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="appDomainFactory">The application domain factory.</param>
        [SecurityCritical]
        internal IsolatedRazorEngineService(IConfigCreator configCreator, IAppDomainFactory appDomainFactory)
        {
            _appDomain = CreateAppDomain(appDomainFactory ?? new DefaultAppDomainFactory());
            var config = configCreator ?? new DefaultConfigCreator();

            string assemblyName = RazorEngineServiceType.Assembly.FullName;
            string typeName = RazorEngineServiceType.FullName;


            ObjectHandle handle = 
                Activator.CreateInstanceFrom(
                    _appDomain, typeof(SanboxHelper).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(SanboxHelper).FullName
                );

            SanboxHelper helper = (SanboxHelper)handle.Unwrap();
            _proxy = helper.CreateEngine(config);
        }

        [SecurityCritical]
        public static IRazorEngineService Create()
        {
            return Create(null, (IAppDomainFactory)null);
        }
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator config)
        {
            return Create(config, (IAppDomainFactory)null);
        }
        [SecurityCritical]
        public static IRazorEngineService Create(IAppDomainFactory appDomainFactory)
        {
            return Create(null, appDomainFactory);
        }
        [SecurityCritical]
        public static IRazorEngineService Create(Func<AppDomain> appDomainFactory)
        {
            return Create(null, new DelegateAppDomainFactory(appDomainFactory));
        }
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator configCreator, Func<AppDomain> appDomainFactory)
        {
            return Create(configCreator, new DelegateAppDomainFactory(appDomainFactory));
        }
        [SecurityCritical]
        public static IRazorEngineService Create(IConfigCreator configCreator, IAppDomainFactory appDomainFactory)
        {
            configCreator = configCreator ?? new DefaultConfigCreator();
            var config = configCreator.CreateConfiguration();
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

        #endregion

        public ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _proxy.GetKey(name, resolveType, context);
        }

        public void CompileAndCache(ITemplateKey key, Type modelType = null)
        {
            _proxy.CompileAndCache(key, modelType);
        }

        public void RunCompileOnDemand(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _proxy.RunCompileOnDemand(key, writer, modelType, model, viewBag);
        }

        public void RunCachedTemplate(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _proxy.RunCachedTemplate(key, writer, modelType, model, viewBag);
        }

        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            return _proxy.IsTemplateCached(key, modelType);
        }

        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            _proxy.AddTemplate(key, templateSource);
        }
    }
}