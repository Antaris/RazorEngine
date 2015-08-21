namespace RazorEngine.Compilation.VisualBasic
{
#if !RAZOR4 // no support for VB.net in Razor4?
    using System.Security;
    using System.Web.Razor;
    using System.Web.Razor.Parser.SyntaxTree;
    using Templating;

    /// <summary>
    /// Defines a code generator that supports VB syntax.
    /// </summary>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    public class VBRazorCodeGenerator : System.Web.Razor.Generator.VBRazorCodeGenerator
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VBRazorCodeGenerator"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rootNamespaceName">Name of the root namespace.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="host">The host.</param>
        /// <param name="strictMode">Flag to specify that this generator is running in struct mode.</param>
        public VBRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host, bool strictMode)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
            StrictMode = strictMode;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the code generator is running in strict mode.
        /// </summary>
        public bool StrictMode { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Visits an error generated through parsing.
        /// </summary>
        /// <param name="err">The error that was generated.</param>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public override void VisitError(RazorError err)
        {
            if (StrictMode)
                throw new TemplateParsingException(err.Message, err.Location.CharacterIndex, err.Location.LineIndex);
        }
        #endregion
    }
#endif
}