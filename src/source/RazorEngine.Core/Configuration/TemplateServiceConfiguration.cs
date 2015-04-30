namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;
    using RazorEngine.Compilation.ReferenceResolver;
    using System.Security;
    using RazorEngine.Configuration.Xml;

    /// <summary>
    /// Provides a default implementation of a template service configuration.
    /// </summary>
    public class TemplateServiceConfiguration : ITemplateServiceConfiguration
    {
#pragma warning disable 0618 // Backwards Compat.
        private ITemplateResolver resolver;
#pragma warning restore 0618 // Backwards Compat.

        #region Constructor

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateServiceConfiguration"/>.
        /// </summary>
        [SecuritySafeCritical]
        public TemplateServiceConfiguration()
        {
            Activator = new DefaultActivator();
            CompilerServiceFactory = new DefaultCompilerServiceFactory();
            EncodedStringFactory = new HtmlEncodedStringFactory();
            
#if !RAZOR4
#pragma warning disable 0618 // Backwards Compat.
            CodeInspectors = new List<ICodeInspector>();
#pragma warning restore 0618 // Backwards Compat.
#endif

            ReferenceResolver = new UseCurrentAssembliesReferenceResolver();
            CachingProvider = new DefaultCachingProvider();
            TemplateManager =
                new DelegateTemplateManager();

            Namespaces = new HashSet<string>
                             {
                                 "System", 
                                 "System.Collections.Generic", 
                                 "System.Linq"
                             };
            
            var config = RazorEngineConfigurationSection.GetConfiguration();
            Language = (config == null)
                           ? Language.CSharp
                           : config.DefaultLanguage;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the activator.
        /// </summary>
        public IActivator Activator { get; set; }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        public bool AllowMissingPropertiesOnDynamic { get; set; }

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
        public bool DisableTempFileLocking { get; set; }

        /// <summary>
        /// Gets or sets the base template type.
        /// </summary>
        public Type BaseTemplateType { get; set; }

#if !RAZOR4
#pragma warning disable 0618 // Backwards Compat.
        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        IEnumerable<ICodeInspector> ITemplateServiceConfiguration.CodeInspectors { get { return CodeInspectors; } }
        
        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
        public IList<ICodeInspector> CodeInspectors { get; private set; }
#pragma warning restore 0618 // Backwards Compat.
#endif
        
        /// <summary>
        /// Gets or sets the reference resolver
        /// </summary>
        public IReferenceResolver ReferenceResolver { get; set; }

        /// <summary>
        /// Gets or sets the caching provider.
        /// </summary>
        public ICachingProvider CachingProvider { get; set; }

        /// <summary>
        /// Gets or sets the compiler service factory.
        /// </summary>
        public ICompilerServiceFactory CompilerServiceFactory { get; set; }

        /// <summary>
        /// Gets whether the template service is operating in debug mode.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets or sets the encoded string factory.
        /// </summary>
        public IEncodedStringFactory EncodedStringFactory { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// Gets or sets the collection of namespaces.
        /// </summary>
        public ISet<string> Namespaces { get; set; }

        /// <summary>
        /// Gets or sets the template resolver.
        /// </summary>
        [Obsolete("Please use the TemplateManager property instead")]
        public ITemplateResolver Resolver { 
            get { 
                return resolver;
            } 
            set { 
                resolver = value;
                if (value != null)
                {
                    TemplateManager = new WrapperTemplateManager(value);
                }
            } 
        }

        /// <summary>
        /// Gets or sets the template resolver.
        /// </summary>
        public ITemplateManager TemplateManager { get; set; }
        #endregion
    }
}