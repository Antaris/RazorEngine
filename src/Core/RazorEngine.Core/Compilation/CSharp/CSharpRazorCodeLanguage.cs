//-----------------------------------------------------------------------------
// <copyright file="CSharpRazorCodeLanguage.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation.CSharp
{
    using System.Web.Razor;
    using System.Web.Razor.Generator;

    /// <summary>
    /// Provides a razor code language that supports the C# language.
    /// </summary>
    public class CSharpRazorCodeLanguage : System.Web.Razor.CSharpRazorCodeLanguage
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpRazorCodeLanguage"/> class.
        /// </summary>
        /// <param name="strictMode">Flag to determine whether strict mode is enabled.</param>
        public CSharpRazorCodeLanguage(bool strictMode)
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
            return new CSharpRazorCodeGenerator(className, rootNamespaceName, sourceFileName, host, this.StrictMode);
        }

        #endregion
    }
}