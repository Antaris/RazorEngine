//-----------------------------------------------------------------------------
// <copyright file="VBRazorCodeLanguage.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation.VisualBasic
{
    using System.Web.Razor;
    using System.Web.Razor.Generator;

    /// <summary>
    /// Provides a razor code language that supports the VB language.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class VBRazorCodeLanguage : System.Web.Razor.VBRazorCodeLanguage
    // ReSharper restore InconsistentNaming
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VBRazorCodeLanguage"/> class.
        /// </summary>
        /// <param name="strictMode">Flag to determine whether strict mode is enabled.</param>
        public VBRazorCodeLanguage(bool strictMode)
        {
            this.StrictMode = strictMode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether strict mode is enabled.
        /// </summary>
        public bool StrictMode { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the code generator.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rootNamespaceName">Name of the root namespace.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="host">The host.</param>
        /// <returns>An instance of <see cref="RazorCodeGenerator"/>.</returns>
        public override RazorCodeGenerator CreateCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
        {
            return new VBRazorCodeGenerator(className, rootNamespaceName, sourceFileName, host, this.StrictMode);
        }

        #endregion
    }
}