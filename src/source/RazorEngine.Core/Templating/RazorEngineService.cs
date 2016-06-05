using RazorEngine.Configuration;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
#if RAZOR4
using System.Runtime.ExceptionServices;
#endif
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{

    /// <summary>
    /// Defines a template service and the main API for running templates.
    /// Implements the <see cref="IRazorEngineService"/> interface.
    /// </summary>
    public class RazorEngineService : CrossAppDomainObject, IRazorEngineService
    {
        #region Fields
        private readonly ITemplateServiceConfiguration _config;
        //private readonly RazorEngineCore _core;
        private readonly RazorEngineCore _core_with_cache;

        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="config">The template service configuration.</param>
        internal RazorEngineService(ITemplateServiceConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            if (config.Debug && config.DisableTempFileLocking)
            {
                throw new InvalidOperationException("Debug && DisableTempFileLocking is not supported, you need to disable one of them. When Roslyn has been released and you are seeing this, open an issue as this might be possible now.");
            }

            _config = config;
            //_core = new RazorEngineCore(config, this);
            _core_with_cache = new RazorEngineCoreWithCache(new ReadOnlyTemplateServiceConfiguration(config), this);
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

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance.
        /// </summary>
        /// <returns></returns>
        public static IRazorEngineService Create()
        {
            return Create(new TemplateServiceConfiguration());
        }

        /// <summary>
        /// Creates a new <see cref="IRazorEngineService"/> instance with the given configuration.
        /// </summary>
        /// <returns></returns>
        public static IRazorEngineService Create(ITemplateServiceConfiguration config)
        {
            return new DynamicWrapperService(new RazorEngineService(config), false, config.AllowMissingPropertiesOnDynamic);
        }
        #endregion

        #region Properties

        /// <summary>
        /// The internal core instance.
        /// </summary>
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


        /// <summary>
        /// Checks if a given template is already cached in the <see cref="ICachingProvider"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            ICompiledTemplate template;
            return Configuration.CachingProvider.TryRetrieveTemplate(key, modelType, out template);
        }

        /// <summary>
        /// Adds a given template to the <see cref="ITemplateManager"/> as dynamic template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            Configuration.TemplateManager.AddDynamic(key, templateSource);
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing">Are we explicitly disposing of this instance?</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                _config.CachingProvider.Dispose();
                _core_with_cache.Dispose();
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
        /// Compiles the given template, caches it and returns the result.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        internal ICompiledTemplate CompileAndCacheInternal(ITemplateKey key, Type modelType)
        {
            var compiled = _core_with_cache.Compile(key, modelType);
            _config.CachingProvider.CacheTemplate(compiled, key);
            return compiled;
        }

        /// <summary>
        /// Compiles the specified template and caches it.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="modelType">The model type.</param>
        public void Compile(ITemplateKey key, Type modelType = null)
        {
            CompileAndCacheInternal(key, modelType);
        }

        /// <summary>
        /// Tries to resolve the template.
        /// When the cache misses we either throw an exception or compile the template.
        /// Otherwise the result is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="compileOnCacheMiss"></param>
        /// <returns></returns>
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
            var template = GetCompiledTemplate(key, modelType, true);

            Run(key, writer, modelType, model, viewBag);
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
            var template = GetCompiledTemplate(key, modelType, false);
#if RAZOR4
            try
            {
                _core_with_cache.RunTemplate(template, writer, model, viewBag).Wait();
            }
            catch (AggregateException ex)
            {
                ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
            }
#else
            _core_with_cache.RunTemplate(template, writer, model, viewBag);
#endif
        }

        /// <summary>
        /// Helper method for the legacy <see cref="TemplateService"/> class.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewbag"></param>
        /// <returns></returns>
        internal ITemplate GetTemplate(ITemplateKey key, Type modelType, object model, DynamicViewBag viewbag)
        {
            var template = GetCompiledTemplate(key, modelType, true);
            return _core_with_cache.CreateTemplate(template, model, viewbag);
        }

        /// <summary>
        /// Gets a given key from the <see cref="ITemplateManager"/> implementation.
        /// See <see cref="ITemplateManager.GetKey"/>.
        /// </summary>
        /// <param name="cacheName"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ITemplateKey GetKey(string cacheName, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _core_with_cache.GetKey(cacheName, resolveType, context);
        }
        #endregion
    }
}
