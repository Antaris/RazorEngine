//-----------------------------------------------------------------------------
// <copyright file="VBCodeParser.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation.VisualBasic
{
    using System.Diagnostics.Contracts;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;
    using Spans;

    /// <summary>
    /// Defines a code parser that supports the VB syntax.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class VBCodeParser : System.Web.Razor.Parser.VBCodeParser
    // ReSharper restore InconsistentNaming
    {
        #region Fields

        /// <summary>
        /// The flag
        /// </summary>
        private bool modelOrInheritsStatementFound;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VBCodeParser"/> class.
        /// </summary>
        public VBCodeParser()
        {
            this.KeywordHandlers.Add("modeltype", this.ParseModelTypeStatement);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses the model type statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        /// <returns>Always return false</returns>
        public bool ParseModelTypeStatement(CodeBlockInfo block)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(block != null);
            /* ReSharper restore InvocationIsSkipped */

            using (StartBlock(BlockType.Directive))
            {
                block.ResumeSpans(Context);

                SourceLocation location = CurrentLocation;
                bool readWhitespace = RequireSingleWhiteSpace();

                End(MetaCodeSpan.Create(Context, false, readWhitespace ? AcceptedCharacters.None : AcceptedCharacters.Any));

                if (this.modelOrInheritsStatementFound)
                {
                    OnError(location, "The modeltype or inherits keywords can only appear once.");
                }

                this.modelOrInheritsStatementFound = true;

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

        /// <summary>
        /// Parses the inherits statement.
        /// </summary>
        /// <param name="block">The code block.</param>
        /// <returns>Always return true</returns>
        protected override bool ParseInheritsStatement(CodeBlockInfo block)
        {
            var location = CurrentLocation;

            if (this.modelOrInheritsStatementFound)
            {
                OnError(location, "The modeltype or inherits keywords can only appear once.");
            }

            this.modelOrInheritsStatementFound = true;

            return base.ParseInheritsStatement(block);
        }
        #endregion
    }
}
