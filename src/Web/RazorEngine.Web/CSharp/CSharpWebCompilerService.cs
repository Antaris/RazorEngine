namespace RazorEngine.Web.CSharp
{
    using System;
    using System.Linq;
    using System.Web.Razor.Parser;

    using Compilation;
    using Compilation.CSharp;

    /// <summary>
    /// Provides a compiler service for Razor templates with C# syntax running in ASP.NET under a trust level that is not Full.
    /// </summary>
    public class CSharpWebCompilerService : WebCompilerServiceBase
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="CSharpWebCompilerService"/>.
        /// </summary>
        public CSharpWebCompilerService(bool strictMode = false, Func<MarkupParser> markupParser = null) : base("csrzr", new CSharpRazorCodeLanguage(strictMode), markupParser) { }
        #endregion

        #region Methods
        /// <summary>
        /// Builds a type name for the specified generic type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The string typename (including namespace and generic type parameters).
        /// </returns>
        public override string BuildTypeNameInternal(Type type, bool isDynamic)
        {
            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace
                   + "."
                   + type.Name.Substring(0, type.Name.IndexOf('`'))
                   + "<"
                   + (isDynamic ? "dynamic" : string.Join(", ", type.GetGenericArguments().Select(t => BuildTypeNameInternal(t, CompilerServicesUtility.IsDynamicType(t)))))
                   + ">";
        }
        #endregion
    }
}