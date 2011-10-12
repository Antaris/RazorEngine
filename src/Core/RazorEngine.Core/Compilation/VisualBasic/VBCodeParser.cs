namespace RazorEngine.Compilation.VisualBasic
{
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    using Spans;

    /// <summary>
    /// Defines a code parser that supports the VB syntax.
    /// </summary>
    public class VBCodeParser : System.Web.Razor.Parser.VBCodeParser
    {
        #region Fields
        private bool _modelOrInheritsStatementFound;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="VBCodeParser"/>
        /// </summary>
        public VBCodeParser()
        {
            KeywordHandlers.Add("modeltype", ParseModelTypeStatement);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the inherits statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        protected override bool ParseInheritsStatement(CodeBlockInfo block)
        {
            var location = CurrentLocation;

            if (_modelOrInheritsStatementFound)
                OnError(location, "The modeltype or inherits keywords can only appear once.");

            _modelOrInheritsStatementFound = true;

            return base.ParseInheritsStatement(block);
        }

        /// <summary>
        /// Parses the modeltype statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        public bool ParseModelTypeStatement(CodeBlockInfo block)
        {
            using (StartBlock(BlockType.Directive))
            {
                block.ResumeSpans(Context);

                SourceLocation location = CurrentLocation;
                bool readWhitespace = RequireSingleWhiteSpace();

                End(MetaCodeSpan.Create(Context, false, readWhitespace ? AcceptedCharacters.None : AcceptedCharacters.Any));

                if (_modelOrInheritsStatementFound)
                    OnError(location, "The modeltype or inherits keywords can only appear once.");

                _modelOrInheritsStatementFound = true;

                // Accept Whitespace up to the new line or non-whitespace character
                Context.AcceptWhiteSpace(false);

                string typeName = null;
                if (ParserHelpers.IsIdentifierStart(CurrentCharacter))
                {
                    using (Context.StartTemporaryBuffer())
                    {
                        Context.AcceptUntil(ParserHelpers.IsNewLine);
                        typeName = Context.ContentBuffer.ToString();
                        Context.AcceptTemporaryBuffer();
                    }
                    Context.AcceptNewLine();
                }
                else
                {
                    OnError(location, "Expected model identifier.");
                }
                End(new ModelSpan(Context, typeName));
            }
            return false;
        }
        #endregion
    }
}
