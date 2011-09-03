namespace RazorEngine.Parsing
{
    using System.Web.Razor.Parser;

    /// <summary>
    /// Represents a dynamic code span parser.
    /// </summary>
    public interface ICodeSpanParser
    {
        #region Properties
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        string Identifier { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the characters in the current context.
        /// </summary>
        /// <param name="parser">The code parser.</param>
        /// <param name="info">The code block info.</param>
        /// <returns>True if the span was successfully parsed, otherwise false.</returns>
        bool Parse(CodeParser parser, CodeBlockInfo info);
        #endregion
    }
}