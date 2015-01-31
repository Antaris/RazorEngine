namespace RazorEngine.Compilation.Inspectors
{
    using System;
    using System.CodeDom;
#if !RAZOR4
    /// <summary>
    /// Defines the required contract for implementing a code inspector.
    /// </summary>
    [Obsolete("This API is obsolete and will be removed in the next version (Razor4 doesn't use CodeDom for code-generation)!")]
    public interface ICodeInspector
    {
        #region Methods
        /// <summary>
        /// Inspects the specified code unit.
        /// </summary>
        /// <param name="unit">The code unit.</param>
        /// <param name="ns">The code namespace declaration.</param>
        /// <param name="type">The code type declaration.</param>
        /// <param name="executeMethod">The code method declaration for the Execute method.</param>
        void Inspect(CodeCompileUnit unit, CodeNamespace ns, CodeTypeDeclaration type, CodeMemberMethod executeMethod);
        #endregion
    }
#endif
}