namespace RazorEngine.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Defines the main configuration section for the RazorEngine.
    /// </summary>
    public class RazorEngineConfigurationSection : ConfigurationSection
    {
        #region Fields
        private const string ActivatorAttribute = "activator";
        private const string DefaultLanguageAttribute = "defaultLanguage";
        private const string FactoryAttribute = "factory";
        private const string SectionPath = "razorEngine";
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the activator type.
        /// </summary>
        [ConfigurationProperty(ActivatorAttribute, IsRequired = false)]
        public string ActivatorType
        {
            get { return (string)this[ActivatorAttribute]; }
            set { this[ActivatorAttribute] = value; }
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
        /// Gets or sets the factory type.
        /// </summary>
        [ConfigurationProperty(FactoryAttribute, IsRequired = false)]
        public string FactoryType
        {
            get { return (string)this[FactoryAttribute]; }
            set { this[FactoryAttribute] = value; }
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