﻿namespace RazorEngine.Compilation.VisualBasic
{
    using System.CodeDom;
    using System.Web.Razor;
    using System.Web.Razor.Parser.SyntaxTree;
    using Templating;

    /// <summary>
    /// Defines a code generator that supports VB syntax.
    /// </summary>
    public class VBRazorCodeGenerator : System.Web.Razor.Generator.VBRazorCodeGenerator
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VBRazorCodeGenerator"/> class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rootNamespaceName">Name of the root namespace.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="host">The host.</param>
        /// <param name="strictMode">Flag to specify that this generator is running in struct mode.</param>
        public VBRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host, bool strictMode)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
            StrictMode = strictMode;
            var mvcHost = host as Compilation.RazorEngineHost;
            if (mvcHost != null)
            {
                SetBaseTypeFromHost(mvcHost);
            }
        }

        private void SetBaseTypeFromHost(Compilation.RazorEngineHost mvcHost)
        {
            if (!mvcHost.DefaultBaseTemplateType.IsGenericType)
            {
                SetBaseType(mvcHost.DefaultBaseTemplateType.FullName);
            }
            else
            {
                var modelTypeName = CompilerServicesUtility.ResolveVBTypeName(mvcHost.DefaultModelType);
                SetBaseType(mvcHost.DefaultBaseClass + "(Of " + modelTypeName + ")");
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets whether the code generator is running in strict mode.
        /// </summary>
        public bool StrictMode { get; private set; }
        #endregion

        #region Methods
        private void SetBaseType(string baseTypeName)
        {
            var baseType = new CodeTypeReference(baseTypeName);
            Context.GeneratedClass.BaseTypes.Clear();
            Context.GeneratedClass.BaseTypes.Add(baseType);
        }

        /// <summary>
        /// Visits an error generated through parsing.
        /// </summary>
        /// <param name="err">The error that was generated.</param>
        public override void VisitError(RazorError err)
        {
            if (StrictMode)
                throw new TemplateParsingException(err);
        }
        #endregion
    }
}