namespace RazorEngine.Compilation
{
    /// <summary>
    /// Defines the required contract for implementing a compiler service factory.
    /// </summary>
    public interface ICompilerServiceFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="ICompilerService"/> that supports the specified language.
        /// </summary>
        /// <param name="language">The <see cref="Language"/>.</param>
        /// <returns>An instance of <see cref="ICompilerService"/>.</returns>
        ICompilerService CreateCompilerService(Language language);
        #endregion
    }
}
