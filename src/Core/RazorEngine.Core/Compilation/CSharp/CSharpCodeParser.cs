namespace RazorEngine.Compilation.CSharp
{
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;

    using Spans;

    /// <summary>
    /// Defines a code parser that supports the C# syntax.
    /// </summary>
    public class CSharpCodeParser : System.Web.Razor.Parser.CSharpCodeParser
    {
        #region Fields
        private bool _modelOrInheritsStatementFound;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="CSharpCodeParser"/>.
        /// </summary>
        public CSharpCodeParser()
        {
            RazorKeywords.Add("model", WrapSimpleBlockParser(BlockType.Directive, ParseModelStatement));
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
                OnError(location, "The model or inherits keywords can only appear once.");

            _modelOrInheritsStatementFound = true;

            return base.ParseInheritsStatement(block);
        }

        /// <summary>
        /// Parses the model statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        private bool ParseModelStatement(CodeBlockInfo block)
        {
            var location = CurrentLocation;

            bool readWhiteSpace = RequireSingleWhiteSpace();
            End(MetaCodeSpan.Create(Context, false, readWhiteSpace ? AcceptedCharacters.None : AcceptedCharacters.Any));

            if (_modelOrInheritsStatementFound)
                OnError(location, "The model or inherits keywords can only appear once.");

            _modelOrInheritsStatementFound = true;

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
            return false;
        }
        #endregion
    }
}