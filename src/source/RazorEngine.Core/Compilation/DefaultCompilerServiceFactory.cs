namespace RazorEngine.Compilation
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using CSharp;
    using VisualBasic;
    using System.Security;

    /// <summary>
    /// Provides a default implementation of a <see cref="ICompilerServiceFactory"/>.
    /// </summary>
    [Serializable]
    public class DefaultCompilerServiceFactory : ICompilerServiceFactory
    {
        #region Methods
        /// <summary>
        /// Creates a <see cref="ICompilerService"/> that supports the specified language.
        /// </summary>
        /// <param name="language">The <see cref="Language"/>.</param>
        /// <returns>An instance of <see cref="ICompilerService"/>.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SecuritySafeCritical]
        public ICompilerService CreateCompilerService(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpDirectCompilerService();

                case Language.VisualBasic:
#if RAZOR4
                    throw new NotSupportedException("Razor4 doesn't support VB.net apparently.");
#else
                    return new VBDirectCompilerService();
#endif

                default:
                    throw new ArgumentException("Unsupported language: " + language);
            }
        }
        #endregion
    }
}
