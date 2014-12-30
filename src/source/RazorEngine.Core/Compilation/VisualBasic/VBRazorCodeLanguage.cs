namespace RazorEngine.Compilation.VisualBasic
{
#if !RAZOR4 // no support for VB.net in Razor4?
    using System.Security;

    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using OriginalVBRazorCodeLanguage = System.Web.Razor.VBRazorCodeLanguage;

    /// <summary>
    /// Provides a razor code language that supports the VB language.
    /// </summary>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    public class VBRazorCodeLanguage : OriginalVBRazorCodeLanguage
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance
        /// </summary>
        /// <param name="strictMode">Flag to determine whether strict mode is enabled.</param>
        public VBRazorCodeLanguage(bool strictMode)
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
        /// <returns>An instance of <see cref="RazorCodeGenerator"/>.</returns>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public override RazorCodeGenerator CreateCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
        {
            return new VBRazorCodeGenerator(className, rootNamespaceName, sourceFileName, host, StrictMode);
        }
        #endregion
    }
#endif
}