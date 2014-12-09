namespace RazorEngine.Tests.TestTypes.Inspectors
{
    using System.CodeDom;

    using Compilation.Inspectors;

    /// <summary>
    /// Defines a code inspector that will insert a throw statement into the generated code.
    /// </summary>
    public class ThrowExceptionCodeInspector : ICodeInspector
    {
        #region Methods
        /// <summary>
        /// Inspects the specified code unit.
        /// </summary>
        /// <param name="unit">The code unit.</param>
        /// <param name="ns">The code namespace declaration.</param>
        /// <param name="type">The code type declaration.</param>
        /// <param name="executeMethod">The code method declaration for the Execute method.</param>
        public void Inspect(CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration type, CodeMemberMethod executeMethod)
        {
            var statement = new CodeThrowExceptionStatement(
                new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(System.InvalidOperationException)), new CodeExpression[] {}));

            executeMethod.Statements.Insert(0, statement);
        }
        #endregion
    }
}