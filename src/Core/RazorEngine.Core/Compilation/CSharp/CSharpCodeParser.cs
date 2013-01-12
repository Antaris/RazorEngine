namespace RazorEngine.Compilation.CSharp
{
    using System.Web.Razor.Generator;
    using System.Web.Razor.Text;
    using System.Web.Razor.Parser;
    using CodeGenerators;

    /// <summary>
    /// Defines a code parser that supports the C# syntax.
    /// </summary>
    public class CSharpCodeParser : System.Web.Razor.Parser.CSharpCodeParser
    {
        #region Fields
        private const string GenericTypeFormatString = "{0}<{1}>";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="CSharpCodeParser"/>.
        /// </summary>
        public CSharpCodeParser()
        {
            MapDirectives(ModelDirective, "model");
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the inherits statement.
        /// </summary>
        protected override void InheritsDirective()
        {
            // Verify we're on the right keyword and accept
            AssertDirective(SyntaxConstants.CSharp.InheritsKeyword);
            AcceptAndMoveNext();
            _endInheritsLocation = CurrentLocation;

            InheritsDirectiveCore();
            CheckForInheritsAndModelStatements();
        }

        private void CheckForInheritsAndModelStatements()
        {
            if (_modelStatementFound && _endInheritsLocation.HasValue)
            {
                Context.OnError(_endInheritsLocation.Value, "The 'inherits' keyword is not allowed when a 'model' keyword is used.");
            }
        }

        /// <summary>
        /// Parses the model statement.
        /// </summary>
        protected virtual void ModelDirective()
        {
            // Verify we're on the right keyword and accept
            AssertDirective("model");
            AcceptAndMoveNext();

            SourceLocation endModelLocation = CurrentLocation;

            BaseTypeDirective("The 'model' keyword must be followed by a type name on the same line.", CreateModelCodeGenerator);

            if (_modelStatementFound)
            {
                Context.OnError(endModelLocation, "Only one 'model' statement is allowed in a file.");
            }

            _modelStatementFound = true;

            CheckForInheritsAndModelStatements();
        }

        private SpanCodeGenerator CreateModelCodeGenerator(string model)
        {
            return new SetModelTypeCodeGenerator(model, GenericTypeFormatString);
        }
        #endregion
    }
}