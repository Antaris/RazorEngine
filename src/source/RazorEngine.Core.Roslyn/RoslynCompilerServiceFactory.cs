using RazorEngine.Compilation;
using RazorEngine.Roslyn.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Roslyn
{
    /// <summary>
    /// Provides a implementation of <see cref="ICompilerServiceFactory"/> for the Roslyn implementation.
    /// </summary>
    [Serializable]
    public class RoslynCompilerServiceFactory : ICompilerServiceFactory
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
                    return new CSharpRoslynCompilerService();

                case Language.VisualBasic:
//#if RAZOR4
                    throw new NotSupportedException("Razor4 doesn't support VB.net apparently.");
//#else
//                    return new VBRoslynCompilerService();
//#endif

                default:
                    throw new ArgumentException("Unsupported language: " + language);
            }
        }
        #endregion
    }
}
