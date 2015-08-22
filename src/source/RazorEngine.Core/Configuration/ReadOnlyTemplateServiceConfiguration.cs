using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Compilation;
using RazorEngine.Compilation.Inspectors;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace RazorEngine.Configuration
{
    /// <summary>
    /// Provides a readonly view of a configuration, and safe-copies all references.
    /// </summary>
    public class ReadOnlyTemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        private readonly IActivator _activator;
        private readonly bool _allowMissingPropertiesOnDynamic;
        private readonly Type _baseTemplateType;
        private readonly ICachingProvider _cachingProvider;
#if !RAZOR4
        [Obsolete("This API is obsolete and will be removed in the next version(Razor4 doesn't use CodeDom for code-generation)!")]
        private readonly IEnumerable<ICodeInspector> _codeInspectors;
#endif
        private readonly ICompilerServiceFactory _compilerServiceFactory;
        private readonly bool _debug;
        private readonly bool _disableTempFileLocking;
        private readonly IEncodedStringFactory _encodedStringFactory;
        private readonly Language _language;
        private readonly ISet<string> _namespaces;
        private readonly IReferenceResolver _referenceResolver;
        [Obsolete("Use TemplateManager instead")]
        private readonly ITemplateResolver _resolver;
        private readonly ITemplateManager _templateManager;

        /// <summary>
        /// Create a new readonly view (and copy) of the given configuration.
        /// </summary>
        /// <param name="config">the configuration to copy.</param>
        public ReadOnlyTemplateServiceConfiguration(ITemplateServiceConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            _allowMissingPropertiesOnDynamic = config.AllowMissingPropertiesOnDynamic;

            _activator = config.Activator;
            if (_activator == null)
            {
                throw new ArgumentNullException("config", "the configured Activator cannot be null!");
            }

            _baseTemplateType = config.BaseTemplateType;
            // Check if the baseTemplateType is valid.
            if (_baseTemplateType != null && (!typeof(ITemplate).IsAssignableFrom(_baseTemplateType) || typeof(ITemplate) == _baseTemplateType))
            {
                throw new ArgumentOutOfRangeException("config", "the configured BaseTemplateType must implement ITemplate!");
            }
            
            _cachingProvider = config.CachingProvider;
            if (_cachingProvider == null)
            {
                throw new ArgumentNullException("config", "the configured CachingProvider cannot be null!");
            }

#if !RAZOR4
#pragma warning disable 0618 // Backwards Compat.
            _codeInspectors = config.CodeInspectors;
            if (_codeInspectors == null)
            {
                throw new ArgumentNullException("config", "the configured CodeInspectos cannot be null!");
            }
            _codeInspectors = new List<ICodeInspector>(_codeInspectors);
#pragma warning restore 0618 // Backwards Compat.
#endif

            _compilerServiceFactory = config.CompilerServiceFactory;
            if (_compilerServiceFactory == null)
            {
                throw new ArgumentNullException("config", "the configured CompilerServiceFactory cannot be null!");
            }

            _debug = config.Debug;
            _disableTempFileLocking = config.DisableTempFileLocking;
            _encodedStringFactory = config.EncodedStringFactory;
            if (_encodedStringFactory == null)
            {
                throw new ArgumentNullException("config", "the configured EncodedStringFactory cannot be null!");
            }

            _language = config.Language;
            _namespaces = config.Namespaces;
            if (_namespaces == null)
            {
                throw new ArgumentNullException("config", "the configured Namespaces cannot be null!");
            }
            _namespaces = new HashSet<string>(_namespaces);

            _referenceResolver = config.ReferenceResolver;
            if (_referenceResolver == null)
            {
                throw new ArgumentNullException("config", "the configured ReferenceResolver cannot be null!");
            }

#pragma warning disable 0618 // Backwards Compat.
            _resolver = config.Resolver;
            _templateManager = config.TemplateManager;
            if (_templateManager == null)
            {
                if (_resolver != null)
                {
                    _templateManager = new Xml.WrapperTemplateManager(_resolver);
                }
                else
                {
                    throw new ArgumentNullException("config", "the configured TemplateManager and Resolver cannot be null!");
                }
            }
#pragma warning restore 0618 // Backwards Compat.

        }

        /// <summary>
        /// Gets the activator.
        /// </summary>
        public IActivator Activator
        {
            get
            {
                return _activator;
            }
        }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        public bool AllowMissingPropertiesOnDynamic
        {
            get
            {
                return _allowMissingPropertiesOnDynamic;
            }
        }

        /// <summary>
        /// Gets the base template type.
        /// </summary>
        public Type BaseTemplateType
        {
            get
            {
                return _baseTemplateType;
            }
        }

        /// <summary>
        /// Gets the caching provider.
        /// </summary>
        public ICachingProvider CachingProvider
        {
            get
            {
                return _cachingProvider;
            }
        }

#if !RAZOR4
        /// <summary>
        /// Gets the code inspectors.
        /// </summary>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        public IEnumerable<ICodeInspector> CodeInspectors
        {
            get
            {
                return _codeInspectors;
            }
        }
#endif

        /// <summary>
        /// Gets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory
        {
            get
            {
                return _compilerServiceFactory;
            }
        }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug
        {
            get
            {
                return _debug;
            }
        }

        /// <summary>
        /// Loads all dynamic assemblies with Assembly.Load(byte[]).
        /// This prevents temp files from being locked (which makes it impossible for RazorEngine to delete them).
        /// At the same time this completely shuts down any sandboxing/security.
        /// Use this only if you have a limited amount of static templates (no modifications on rumtime), 
        /// which you fully trust and when a seperate AppDomain is no solution for you!.
        /// This option will also hurt debugging.
        /// 
        /// OK, YOU HAVE BEEN WARNED.
        /// </summary>
        public bool DisableTempFileLocking
        {
            get
            {
                return _disableTempFileLocking;
            }
        }

        /// <summary>
        /// Gets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory
        {
            get
            {
                return _encodedStringFactory;
            }
        }

        /// <summary>
        /// Gets the language.
        /// </summary>
        public Language Language
        {
            get
            {
                return _language;
            }
        }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        public ISet<string> Namespaces
        {
            get
            {
                return _namespaces;
            }
        }

        /// <summary>
        /// Gets the reference resolver.
        /// </summary>
        public IReferenceResolver ReferenceResolver
        {
            get
            {
                return _referenceResolver;
            }
        }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        [Obsolete("use TemplateManager instead")]
        public ITemplateResolver Resolver
        {
            get
            {
                return _resolver;
            }
        }

        /// <summary>
        /// Gets the template resolver.
        /// </summary>
        public ITemplateManager TemplateManager
        {
            get
            {
                return _templateManager;
            }
        }
    }
}
