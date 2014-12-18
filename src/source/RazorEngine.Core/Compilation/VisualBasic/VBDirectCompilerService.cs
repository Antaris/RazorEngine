namespace RazorEngine.Compilation.VisualBasic
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Razor.Parser;

    using Microsoft.VisualBasic;
    using System.Security;

    /// <summary>
    /// Defines a direct compiler service for the VB syntax.
    /// </summary>
    public class VBDirectCompilerService : DirectCompilerServiceBase
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="VBDirectCompilerService"/>.
        /// </summary>
        /// <param name="strictMode">Specifies whether the strict mode parsing is enabled.</param>
        /// <param name="markupParserFactory">The markup parser to use.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in base class: DirectCompilerServiceBase")]
        [SecurityCritical]
        public VBDirectCompilerService(bool strictMode = true, Func<ParserBase> markupParserFactory = null)
            : base(
                new VBRazorCodeLanguage(strictMode),
                new VBCodeProvider(),
                markupParserFactory) { }
        #endregion

        public override string SourceFileExtension
        {
            get
            {
                return "vb";
            }
        }
    }
}