namespace RazorEngine.Compilation.VisualBasic
{
#if !RAZOR4 // no support for VB.net in Razor4?
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
#if RAZOR4
    using Microsoft.AspNet.Razor.Parser;
#else
    using System.Web.Razor.Parser;
#endif

    using Microsoft.VisualBasic;
    using System.Security;

    /// <summary>
    /// Defines a direct compiler service for the VB syntax.
    /// </summary>
    public class VBDirectCompilerService : DirectCompilerServiceBase
    {
    #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="VBDirectCompilerService"/>.
        /// </summary>
        /// <param name="strictMode">Specifies whether the strict mode parsing is enabled.</param>
        /// <param name="markupParserFactory">The markup parser to use.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in base class: DirectCompilerServiceBase")]
        [SecurityCritical]
        public VBDirectCompilerService(bool strictMode = true, Func<ParserBase> markupParserFactory = null)
            : base(
                new VBRazorCodeLanguage(strictMode),
                new VBCodeProvider(),
                markupParserFactory) { }
    #endregion

        /// <summary>
        /// Extension of a source file without dot ("cs" for C# files or "vb" for VB.NET files).
        /// </summary>
        public override string SourceFileExtension
        {
            get
            {
                return "vb";
            }
        }
    
        /// <summary>
        /// Builds a type name for the specified template type.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>The string type name (including namespace).</returns>
        [Pure]
        public override string BuildTypeName(Type templateType, Type modelType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            var modelTypeName = CompilerServicesUtility.ResolveVBTypeName(modelType);
            return CompilerServicesUtility.VBCreateGenericType(templateType, modelTypeName, false);
        }

    }
#endif
}