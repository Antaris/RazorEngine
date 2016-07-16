namespace RazorEngine.Compilation
{
    using System;
    using System.Security;
#if RAZOR4
    using Microsoft.AspNetCore.Razor;
    using Microsoft.AspNetCore.Razor.Parser;
    using OriginalRazorEngineHost = Microsoft.AspNetCore.Razor.RazorEngineHost;
#else
    using System.Web.Razor;
    using System.Web.Razor.Parser;
    using OriginalRazorEngineHost = System.Web.Razor.RazorEngineHost;
#endif

    /// <summary>
    /// Defines the custom razor engine host.
    /// </summary>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    public class RazorEngineHost : OriginalRazorEngineHost
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="RazorEngineHost"/>.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="markupParserFactory">The markup parser factory delegate.</param>
        public RazorEngineHost(RazorCodeLanguage language, Func<ParserBase> markupParserFactory)
            : base(language, markupParserFactory) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the default template type.
        /// </summary>
        public Type DefaultBaseTemplateType { get; set; }

        /// <summary>
        /// Gets or sets the default model type.
        /// </summary>
        public Type DefaultModelType { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Decorates the code parser.
        /// </summary>
        /// <param name="incomingCodeParser">The code parser.</param>
        /// <returns>The decorated parser.</returns>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            if (incomingCodeParser is CSharpCodeParser)
                return new CSharp.CSharpCodeParser();
#if !RAZOR4
            if (incomingCodeParser is VBCodeParser)
                return new VisualBasic.VBCodeParser();
#endif
            
            return base.DecorateCodeParser(incomingCodeParser);
        }
        #endregion
    }
}
