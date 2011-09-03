namespace RazorEngine.Parsing
{
    using System.Web.Razor.Parser;

    /// <summary>
    /// Provides a default implementation of a code span parser.
    /// </summary>
    public abstract class CodeSpanParserBase : ICodeSpanParser
    {
        #region Properties
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public abstract string Identifier { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the next character is whitespace.
        /// </summary>
        protected virtual bool RequireSingleWhiteSpace(ParserContext context)
        {
            return false;
        }

        /// <summary>
        /// Parses the characters in the current context.
        /// </summary>
        /// <param name="parser">The code parser.</param>
        /// <param name="info">The code block info.</param>
        /// <returns>True if the span was successfully parsed, otherwise false.</returns>
        public abstract bool Parse(CodeParser parser, CodeBlockInfo info);
        #endregion
    }
}