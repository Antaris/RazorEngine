﻿using System;
using System.Linq;

namespace RazorEngine.Compilation.CSharp
{
    using System.CodeDom;
    using System.Security;
#if RAZOR4
    using Microsoft.AspNetCore.Razor;
    using Microsoft.AspNetCore.Razor.CodeGenerators;
    using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
    using OriginalCSharpRazorCodeGenerator = Microsoft.AspNetCore.Razor.Chunks.Generators.RazorChunkGenerator;
#else
    using System.Web.Razor;
    using System.Web.Razor.Parser.SyntaxTree;
    using OriginalCSharpRazorCodeGenerator = System.Web.Razor.Generator.CSharpRazorCodeGenerator;
#endif
    using Templating;

    /// <summary>
    /// Defines a code generator that supports C# syntax.
    /// </summary>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
    [SecurityCritical]
#endif
    public class CSharpRazorCodeGenerator : OriginalCSharpRazorCodeGenerator
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpRazorCodeGenerator"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rootNamespaceName">Name of the root namespace.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="host">The host.</param>
        /// <param name="strictMode">Flag to specify that this generator is running in struct mode.</param>
        public CSharpRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host, bool strictMode)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
            StrictMode = strictMode;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the code generator is running in strict mode.
        /// </summary>
        public bool StrictMode { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Visits an error generated through parsing.
        /// </summary>
        /// <param name="err">The error that was generated.</param>
#if NET45 // Razor 2 has [assembly: SecurityTransparent]
        [SecurityCritical]
#endif
        public override void VisitError(RazorError err)
        {
            if (StrictMode)
                throw new TemplateParsingException(err.Message, err.Location.CharacterIndex, err.Location.LineIndex);
        }
        #endregion
    }
}