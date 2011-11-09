namespace RazorEngine.Compilation
{
    using System.Diagnostics.Contracts;

    using Configuration;

    /// <summary>
    /// Manages creation of <see cref="ICompilerService"/> instances.
    /// </summary>
    public static class CompilerServiceBuilder
    {
        #region Fields
        private static ICompilerServiceFactory _factory = new DefaultCompilerServiceFactory();
        private static readonly object sync = new object();
        #endregion

        #region Methods
        /// <summary>
        /// Sets the <see cref="ICompilerServiceFactory"/> used to create compiler service instances.
        /// </summary>
        /// <param name="factory">The compiler service factory to use.</param>
        public static void SetCompilerServiceFactory(ICompilerServiceFactory factory)
        {
            Contract.Requires(factory != null);

            lock (sync)
            {
                _factory = factory;
            }
        }

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for the specfied language.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <returns>The compiler service instance.</returns>
        public static ICompilerService GetCompilerService(Language language)
        {
            lock (sync)
            {
                return _factory.CreateCompilerService(language);
            }
        }

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for the default <see cref="Language"/>.
        /// </summary>
        /// <returns>The compiler service instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ICompilerService GetDefaultCompilerService()
        {
            var config = RazorEngineConfigurationSection.GetConfiguration();
            if (config == null)
                return GetCompilerService(Language.CSharp);

            return GetCompilerService(config.DefaultLanguage);
        }
        #endregion
    }
}