using System.Linq;
using RazorEngine.Configuration.Xml;

namespace RazorEngine.Configuration
{
    using System;
    using System.Collections.Generic;

    using Compilation;
    using Compilation.Inspectors;
    using Templating;
    using Text;

    /// <summary>
    /// Provides a default implementation of a template service configuration.
    /// </summary>
    public class TemplateServiceConfiguration : ITemplateServiceConfiguration
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateServiceConfiguration"/>.
        /// </summary>
        public TemplateServiceConfiguration()
        {
            // Read configuration values from App.config / Web.config
            // and fallback to appropriate defaults. 
            var xmlConfig = new XmlTemplateServiceConfiguration();

            Activator = xmlConfig.Activator ?? new DefaultActivator();
            CompilerServiceFactory = xmlConfig.CompilerServiceFactory ?? new DefaultCompilerServiceFactory();
            EncodedStringFactory = xmlConfig.EncodedStringFactory ?? new HtmlEncodedStringFactory();
            CodeInspectors = xmlConfig.CodeInspectors != null ? xmlConfig.CodeInspectors.ToList()  : new List<ICodeInspector>();
            Language = xmlConfig.Language;

            Namespaces = new HashSet<string>
                             {
                                 "System", 
                                 "System.Collections.Generic", 
                                 "System.Linq"
                             };

            Namespaces.UnionWith(xmlConfig.Namespaces);
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
        /// Gets or sets the base template type.
        /// </summary>
        public Type BaseTemplateType { get; set; }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        IEnumerable<ICodeInspector> ITemplateServiceConfiguration.CodeInspectors { get { return CodeInspectors; } }

        /// <summary>
        /// Gets the set of code inspectors.
        /// </summary>
        public IList<ICodeInspector> CodeInspectors { get; private set; }

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
        public ITemplateResolver Resolver { get; set; }
        #endregion
    }
}