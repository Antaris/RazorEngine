
namespace RazorEngine.Compilation.VisualBasic
{
    using System;
    using System.Linq;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Tokenizer.Symbols;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;
    using CodeGenerators;

    /// <summary>
    /// Defines a code parser that supports the VB syntax.
    /// </summary>
    public class VBCodeParser : System.Web.Razor.Parser.VBCodeParser
    {
        #region Fields
        private const string GenericTypeFormatString = "{0}(Of {1})";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="VBCodeParser"/>
        /// </summary>
        public VBCodeParser()
        {
            MapDirective("ModelType", ModelTypeDirective);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the inherits statement.
        /// </summary>
        protected override bool InheritsStatement()
        {
            // Verify we're on the right keyword and accept
            Assert(VBKeyword.Inherits);
            VBSymbol inherits = CurrentSymbol;
            NextToken();
            _endInheritsLocation = CurrentLocation;
            PutCurrentBack();
            PutBack(inherits);
            EnsureCurrent();

            bool result = base.InheritsStatement();
            CheckForInheritsAndModelStatements();
            return result;
        }

        private void CheckForInheritsAndModelStatements()
        {
            if (_modelStatementFound && _endInheritsLocation.HasValue)
            {
                Context.OnError(_endInheritsLocation.Value, "The 'inherits' keyword is not allowed when a 'ModelType' keyword is used.");
            }
        }

        /// <summary>
        /// Parses the modeltype statement.
        /// </summary>
        protected virtual bool ModelTypeDirective()
        {
            AssertDirective("ModelType");

            Span.CodeGenerator = SpanCodeGenerator.Null;
            Context.CurrentBlock.Type = BlockType.Directive;

            AcceptAndMoveNext();
            SourceLocation endModelLocation = CurrentLocation;

            if (At(VBSymbolType.WhiteSpace))
            {
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
            }

            AcceptWhile(VBSymbolType.WhiteSpace);
            Output(SpanKind.MetaCode);

            if (_modelStatementFound)
            {
                Context.OnError(endModelLocation, "Only one 'ModelType' statement is allowed in a file.");
            }
            _modelStatementFound = true;

            if (EndOfFile || At(VBSymbolType.WhiteSpace) || At(VBSymbolType.NewLine))
            {
                Context.OnError(endModelLocation, "The 'ModelType' keyword must be followed by a type name on the same line.");
            }

            // Just accept to a newline
            AcceptUntil(VBSymbolType.NewLine);
            if (!Context.DesignTimeMode)
            {
                // We want the newline to be treated as code, but it causes issues at design-time.
                Optional(VBSymbolType.NewLine);
            }

            string baseType = String.Concat(Span.Symbols.Select(s => s.Content)).Trim();
            Span.CodeGenerator = new SetModelTypeCodeGenerator(baseType, GenericTypeFormatString);

            CheckForInheritsAndModelStatements();
            Output(SpanKind.Code);
            return false;
        }
        #endregion
    }
}
