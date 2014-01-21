namespace RazorEngine.Configuration
{
    using System.Configuration;

    using Xml;

    /// <summary>
    /// Defines the main configuration section for the RazorEngine.
    /// </summary>
    public class RazorEngineConfigurationSection : ConfigurationSection
    {
        #region Fields
        private const string ActivatorAttribute = "activatorType";
        private const string AllowMissingPropertiesOnDynamicAttribute = "allowMissingPropertiesOnDynamic";
        private const string CompilerServiceFactoryAttribute = "compilerServiceFactoryType";
        private const string DefaultLanguageAttribute = "defaultLanguage";
        private const string NamespacesElement = "namespaces";
        private const string SectionPath = "razorEngine";
        private const string TemplateResolverAttribute = "templateResolverType";
        private const string TemplateServicesElement = "templateServices";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the activator type.
        /// </summary>
        [ConfigurationProperty(ActivatorAttribute, IsRequired = false)]
        public string ActivatorType
        {
            get { return (string)this[ActivatorAttribute]; }
        }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic models.
        /// </summary>
        [ConfigurationProperty(AllowMissingPropertiesOnDynamicAttribute, IsRequired = false, DefaultValue = false)]
        public bool AllowMissingPropertiesOnDynamic
        {
            get { return (bool)this[AllowMissingPropertiesOnDynamicAttribute]; }
        }

        /// <summary>
        /// Gets the compiler service factory type.
        /// </summary>
        [ConfigurationProperty(CompilerServiceFactoryAttribute, IsRequired = false)]
        public string CompilerServiceFactoryType
        {
            get { return (string)this[CompilerServiceFactoryAttribute]; }
        }

        /// <summary>
        /// Gets or sets the default language.
        /// </summary>
        [ConfigurationProperty(DefaultLanguageAttribute, DefaultValue = Language.CSharp, IsRequired = false)]
        public Language DefaultLanguage
        {
            get { return (Language)this[DefaultLanguageAttribute]; }
            set { this[DefaultLanguageAttribute] = value; }
        }

        /// <summary>
        /// Gets the collection of namespaces.
        /// </summary>
        [ConfigurationProperty(NamespacesElement, IsRequired = false)]
        public NamespaceConfigurationElementCollection Namespaces
        {
            get { return (NamespaceConfigurationElementCollection)this[NamespacesElement]; }
        }

        /// <summary>
        /// Gets the template resolver type.
        /// </summary>
        [ConfigurationProperty(TemplateResolverAttribute, IsRequired = false)]
        public string TemplateResolverType
        {
            get { return (string)this[TemplateResolverAttribute]; }
        }

        /// <summary>
        /// Gets the collection of template service configurations.
        /// </summary>
        [ConfigurationProperty(TemplateServicesElement, IsRequired = false)]
        public TemplateServiceConfigurationElementCollection TemplateServices
        {
            get { return (TemplateServiceConfigurationElementCollection)this[TemplateServicesElement]; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an instance of <see cref="RazorEngineConfigurationSection"/> that represents the current configuration.
        /// </summary>
        /// <returns>An instance of <see cref="RazorEngineConfigurationSection"/>, or null if no configuration is specified.</returns>
        public static RazorEngineConfigurationSection GetConfiguration()
        {
            return ConfigurationManager.GetSection(SectionPath) as RazorEngineConfigurationSection;
        }
        #endregion
    }
}