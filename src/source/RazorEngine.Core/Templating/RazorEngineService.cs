using RazorEngine.Configuration;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{

    /// <summary>
    /// Defines a template service.
    /// </summary>
    public class RazorEngineService : MarshalByRefObject, IRazorEngineService
    {
        #region Fields
        private readonly ITemplateServiceConfiguration _config;
        //private readonly RazorEngineCore _core;
        private readonly RazorEngineCore _core_with_cache;

        private static readonly Type _objectType = typeof(object);

        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="config">The template service configuration.</param>
        internal RazorEngineService(ITemplateServiceConfiguration config)
        {
            Contract.Requires(config != null);

            _config = config;
            //_core = new RazorEngineCore(config, this);
            _core_with_cache = new RazorEngineCoreWithCache(config, this);
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        internal RazorEngineService()
            : this(new TemplateServiceConfiguration()) { }


        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">the encoding.</param>
        internal RazorEngineService(Language language, Encoding encoding)
            : this(new TemplateServiceConfiguration() { Language = language, EncodedStringFactory = GetEncodedStringFactory(encoding) }) { }


        public static IRazorEngineService Create()
        {
            return Create(new TemplateServiceConfiguration());
        }
        public static IRazorEngineService Create(ITemplateServiceConfiguration config)
        {
            return new RazorEngineService(config);
        }
        #endregion

        #region Properties

        internal RazorEngineCore Core
        {
            get { return _core_with_cache; }
        }

        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        internal ITemplateServiceConfiguration Configuration { get { return _config; } }

        #endregion

        #region Methods

        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            ICompiledTemplate template;
            return Configuration.CachingProvider.TryRetrieveTemplate(key, modelType, out template);
        }

        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            Configuration.TemplateManager.AddDynamic(key, templateSource);
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
                _config.CachingProvider.Dispose();
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

        internal ICompiledTemplate CompileAndCacheInternal(ITemplateKey key, Type modelType)
        {
            var compiled = _core_with_cache.Compile(key, modelType);
            _config.CachingProvider.CacheTemplate(compiled, key);
            return compiled;
        }

        public void CompileAndCache(ITemplateKey key, Type modelType = null)
        {
            CompileAndCacheInternal(key, modelType);
        }

        internal ICompiledTemplate GetCompiledTemplate(ITemplateKey key, Type modelType, bool compileOnCacheMiss)
        {
            ICompiledTemplate template;

            if (!_config.CachingProvider.TryRetrieveTemplate(key, modelType, out template))
            {
                if (compileOnCacheMiss)
                {
                    template = CompileAndCacheInternal(key, modelType);
                }
                else
                {
                    throw new InvalidOperationException("No template exists with key '" + key.GetUniqueKeyString() + "'");
                }
            }
            return template;
        }

        public void RunCompileOnDemand(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            var template = GetCompiledTemplate(key, modelType, true);
            RunCachedTemplate(key, writer, modelType, model, viewBag);
        }

        public void RunCachedTemplate(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            var template = GetCompiledTemplate(key, modelType, false);
            _core_with_cache.RunTemplate(template, writer, model, viewBag);
        }

        internal ITemplate GetTemplate(ITemplateKey key, Type modelType, object model)
        {
            var template = GetCompiledTemplate(key, modelType, true);
            return _core_with_cache.CreateTemplate(template, model);
        }


        public ITemplateKey GetKey(string cacheName, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _core_with_cache.GetKey(cacheName, resolveType, context);
        }

        internal void RunTemplate(ICompiledTemplate template, System.IO.TextWriter writer, object model, DynamicViewBag viewBag)
        {
            _core_with_cache.RunTemplate(template, writer, model, viewBag);
        }
        #endregion
    }
}
