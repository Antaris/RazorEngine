//-----------------------------------------------------------------------------
// <copyright file="CompilerServiceBuilder.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// Synchronization Lock
        /// </summary>
        private static readonly object Sync = new object();

        /// <summary>
        /// The Compiler Service Factory
        /// </summary>
        private static ICompilerServiceFactory serviceFactory = new DefaultCompilerServiceFactory();

        #endregion

        #region Methods
        /// <summary>
        /// Sets the <see cref="ICompilerServiceFactory"/> used to create compiler service instances.
        /// </summary>
        /// <param name="factory">The compiler service factory to use.</param>
        public static void SetCompilerServiceFactory(ICompilerServiceFactory factory)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(factory != null);
            /* ReSharper restore InvocationIsSkipped */

            lock (Sync)
            {
                CompilerServiceBuilder.serviceFactory = factory;
            }
        }

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for the specified language.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <returns>The compiler service instance.</returns>
        public static ICompilerService GetCompilerService(Language language)
        {
            lock (Sync)
            {
                return serviceFactory.CreateCompilerService(language);
            }
        }

        /// <summary>
        /// Gets the <see cref="ICompilerService"/> for the default <see cref="Language"/>.
        /// </summary>
        /// <returns>The compiler service instance.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reviewed. Suppression is OK here.")]
        public static ICompilerService GetDefaultCompilerService()
        {
            var config = RazorEngineConfigurationSection.GetConfiguration();
            if (config == null)
            {
                return GetCompilerService(Language.CSharp);
            }

            return GetCompilerService(config.DefaultLanguage);
        }

        #endregion
    }
}