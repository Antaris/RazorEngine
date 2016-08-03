namespace RazorEngine.Compilation.CSharp
{
    using System.Security;
#if RAZOR4
    using Microsoft.AspNetCore.Razor;
    using Microsoft.AspNetCore.Razor.Chunks.Generators;
    using OriginalCSharpRazorCodeLanguage = Microsoft.AspNetCore.Razor.CSharpRazorCodeLanguage;
#else
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using OriginalCSharpRazorCodeLanguage = System.Web.Razor.CSharpRazorCodeLanguage;
#endif

    /// <summary>
    /// Provides a razor code language that supports the C# language.
    /// </summary>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    public class CSharpRazorCodeLanguage : OriginalCSharpRazorCodeLanguage
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance
        /// </summary>
        /// <param name="strictMode">Flag to determine whether strict mode is enabled.</param>
        public CSharpRazorCodeLanguage(bool strictMode)
        {
            StrictMode = strictMode;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether strict mode is enabled.
        /// </summary>
        public bool StrictMode { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the code generator.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rootNamespaceName">Name of the root namespace.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="host">The host.</param>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
#if RAZOR4
        public override RazorChunkGenerator CreateChunkGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
#else
        public override RazorCodeGenerator CreateCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
#endif
        {
            return new CSharpRazorCodeGenerator(className, rootNamespaceName, sourceFileName, host, StrictMode);
        }
        #endregion
    }
}