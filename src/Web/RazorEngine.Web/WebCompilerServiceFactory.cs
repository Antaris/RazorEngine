using RazorEngine.Web.VisualBasic;

namespace RazorEngine.Web
{
    using System;
    using Compilation;
    using CSharp;
    using VisualBasic;

    /// <summary>
    /// Provides a compiler service factory for web compiler services.
    /// </summary>
    public class WebCompilerServiceFactory : ICompilerServiceFactory
    {
        #region Methods
        /// <summary>
        /// Creates an instance of a compiler service.
        /// </summary>
        /// <param name="language">The language to support in templates.</param>
        /// <returns>An instance of <see cref="ICompilerService"/>.</returns>
        public ICompilerService CreateCompilerService(Language language)
        {
            switch (language)
            {
                case Language.CSharp:
                    return new CSharpWebCompilerService();
                case Language.VisualBasic:
                    return new VBWebCompilerService();
            }

            throw new ArgumentException("The language '" + language + "' is not supported.");
        }
        #endregion
    }

}
