namespace RazorEngine.Templating
{
    using Configuration;

    /// <summary>
    /// Provides factory methods for creating <see cref="ITemplateService"/> instances.
    /// </summary>
    public static class TemplateServiceFactory
    {
        #region Fields
        private static readonly RazorEngineConfigurationSection Configuration;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises the <see cref="TemplateServiceFactory"/> type.
        /// </summary>
        static TemplateServiceFactory()
        {
            Configuration = RazorEngineConfigurationSection.GetConfiguration();
        }
        #endregion

        #region Methods
        public static ITemplateService CreateTemplateService(Language language, Encoding encoding)
        {
            return null;
        }
        #endregion
    }
}