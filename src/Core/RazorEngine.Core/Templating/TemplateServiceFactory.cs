//-----------------------------------------------------------------------------
// <copyright file="TemplateServiceFactory.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using Configuration;

    /// <summary>
    /// Provides factory methods for creating <see cref="ITemplateService"/> instances.
    /// </summary>
    public static class TemplateServiceFactory
    {
        #region Fields

        /// <summary>
        /// The Configuration field
        /// </summary>
        private static readonly RazorEngineConfigurationSection Configuration;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes static members of the <see cref="TemplateServiceFactory"/> class. 
        /// </summary>
        static TemplateServiceFactory()
        {
            Configuration = RazorEngineConfigurationSection.GetConfiguration();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of a template service.
        /// </summary>
        /// <param name="language">The language to use in this service.</param>
        /// <param name="encoding">The type of encoding to use in this service.</param>
        /// <returns>The template service instance</returns>
        public static ITemplateService CreateTemplateService(Language language, Encoding encoding)
        {
            return null;
        }

        #endregion
    }
}