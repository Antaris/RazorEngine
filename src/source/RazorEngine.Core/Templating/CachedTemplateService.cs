using RazorEngine.Configuration;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{

    /// <summary>
    /// Defines a template service.
    /// </summary>
    public class CachedTemplateService : MarshalByRefObject, ICachedTemplateService
    {
        #region Fields
        private readonly ITemplateServiceConfiguration _config;
        private readonly TemplateServiceCore _core;
        private readonly TemplateServiceCore _core_with_cache;

        private static readonly Type _objectType = typeof(object);

        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="config">The template service configuration.</param>
        internal CachedTemplateService(ITemplateServiceConfiguration config)
        {
            Contract.Requires(config != null);

            _config = config;
            _core = new TemplateServiceCore(config, this);
            _core_with_cache = new TemplateServiceCoreWithCache(config, this);
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>.
        /// </summary>
        internal CachedTemplateService()
            : this(new TemplateServiceConfiguration()) { }

        public static ICachedTemplateService Create()
        {
            return Create(new TemplateServiceConfiguration());
        }
        public static ICachedTemplateService Create(ITemplateServiceConfiguration config)
        {
            return new CachedTemplateService(config);
        }

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateService"/>
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="encoding">the encoding.</param>
        internal CachedTemplateService(Language language, Encoding encoding)
            : this(new TemplateServiceConfiguration() { Language = language, EncodedStringFactory = GetEncodedStringFactory(encoding) }) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the template service configuration.
        /// </summary>
        public ITemplateServiceConfiguration Configuration { get { return _config; } }

        #endregion

        #region Methods

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


        #endregion

        internal TemplateServiceCore InternalCore
        {
            get { return _core; }
        }


        public ITemplateServiceCore Core
        {
            get { return _core; }
        }

        public ICompiledTemplate CompileAndCache(ITemplateKey key, Type modelType)
        {
            var compiled = _core_with_cache.Compile(key, modelType);
            _config.CachingProvider.CacheTemplate(compiled, key);
            return compiled;
        }

        internal ICompiledTemplate GetCompiledTemplate(ITemplateKey key, Type modelType, bool compileOnCacheMiss)
        {
            ICompiledTemplate template;

            if (!_config.CachingProvider.TryRetrieveTemplate(key, modelType, out template))
            {
                if (compileOnCacheMiss)
                {
                    template = CompileAndCache(key, modelType);
                }
                else
                {
                    throw new InvalidOperationException("No template exists with key '" + key.GetUniqueKeyString() + "'");
                }
            }
            return template;
        }

        public void RunCompileOnDemand(ITemplateKey key, Type modelType, System.IO.TextWriter writer, object model, DynamicViewBag viewBag)
        {
            var template = GetCompiledTemplate(key, modelType, true);
            RunCachedTemplate(key, modelType, writer, model, viewBag);
        }

        public void RunCachedTemplate(ITemplateKey key, Type modelType, System.IO.TextWriter writer, object model, DynamicViewBag viewBag)
        {
            var template = GetCompiledTemplate(key, modelType, false);
            _core_with_cache.RunTemplate(template, writer, model, viewBag);
        }

        internal ITemplate GetTemplate(ITemplateKey key, Type modelType, object model)
        {
            var template = GetCompiledTemplate(key, modelType, true);
            return _core_with_cache.CreateTemplate(template, model);
        }
    }
}
