﻿//-----------------------------------------------------------------------------
// <copyright file="RazorEngineHost.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation
{
    using System;
    using System.Web.Razor;
    using System.Web.Razor.Parser;

    /// <summary>
    /// Defines the custom razor engine host.
    /// </summary>
    public class RazorEngineHost : System.Web.Razor.RazorEngineHost
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RazorEngineHost"/> class.
        /// </summary>
        /// <param name="language">The code language.</param>
        /// <param name="markupParserFactory">The markup parser factory delegate.</param>
        public RazorEngineHost(RazorCodeLanguage language, Func<MarkupParser> markupParserFactory)
            : base(language, markupParserFactory)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default template type.
        /// </summary>
        public Type DefaultBaseTemplateType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Decorates the code parser.
        /// </summary>
        /// <param name="incomingCodeParser">The code parser.</param>
        /// <returns>The decorated parser.</returns>
        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            if (incomingCodeParser is CSharpCodeParser)
            {
                return new CSharp.CSharpCodeParser();
            }

            if (incomingCodeParser is VBCodeParser)
            {
                return new VisualBasic.VBCodeParser();
            }

            return incomingCodeParser;
        }

        #endregion
    }
}
