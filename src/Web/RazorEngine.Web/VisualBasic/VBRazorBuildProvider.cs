namespace RazorEngine.Web.VisualBasic
{
    using System.Web.Compilation;

    /// <summary>
    /// Defines a build provider for compiling Razor templates with VB syntax.
    /// </summary>
    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    public class VBRazorBuildProvider : RazorBuildProviderBase
    {
        #region Constructors
        /// <summary>
        /// Initialises a new instance of <see cref="VBRazorBuildProvider"/>
        /// </summary>
        public VBRazorBuildProvider() : base(new VBWebCompilerService()) { }
        #endregion

        #region Methods
        /// <summary>
        /// Represents the compiler type used by a build provider to generate source code for a custom file type.
        /// </summary>
        /// <value></value>
        /// <returns>A read-only <see cref="T:System.Web.Compilation.CompilerType"/> representing the code generator, code compiler, and compiler settings used to build source code for the virtual path. The base class returns null.</returns>
        public override CompilerType CodeCompilerType
        {
            get { return GetDefaultCompilerTypeForLanguage("VB"); }
        }
        #endregion
    }
}