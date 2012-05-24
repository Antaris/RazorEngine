//-----------------------------------------------------------------------------
// <copyright file="CSharpCodeParser.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        /// <summary>
        /// The flag
        /// </summary>
        private bool modelOrInheritsStatementFound;

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpCodeParser"/> class.
        /// </summary>
        public CSharpCodeParser()
        {
            this.RazorKeywords.Add("model", this.WrapSimpleBlockParser(BlockType.Directive, this.ParseModelStatement));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the inherits statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        /// <returns>
        /// The parse inherits statement.
        /// </returns>
        protected override bool ParseInheritsStatement(CodeBlockInfo block)
        {
            var location = CurrentLocation;

            if (this.modelOrInheritsStatementFound)
            {
                OnError(location, "The model or inherits keywords can only appear once.");
            }

            this.modelOrInheritsStatementFound = true;

            return base.ParseInheritsStatement(block);
        }

        /// <summary>
        /// Parses the model statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        /// <returns>Returns always false</returns>
        private bool ParseModelStatement(CodeBlockInfo block)
        {
            var location = CurrentLocation;

            bool readWhiteSpace = RequireSingleWhiteSpace();
            End(MetaCodeSpan.Create(Context, false, readWhiteSpace ? AcceptedCharacters.None : AcceptedCharacters.Any));

            if (this.modelOrInheritsStatementFound)
            {
                OnError(location, "The model or inherits keywords can only appear once.");
            }

            this.modelOrInheritsStatementFound = true;

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