//-----------------------------------------------------------------------------
// <copyright file="VBRazorCodeGenerator.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation.VisualBasic
{
    using System.CodeDom;
    using System.Web.Razor;
    using System.Web.Razor.Parser.SyntaxTree;

    using Spans;
    using Templating;

    /// <summary>
    /// Defines a code generator that supports VB syntax.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class VBRazorCodeGenerator : System.Web.Razor.Generator.VBRazorCodeGenerator
    // ReSharper restore InconsistentNaming
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
            this.StrictMode = strictMode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the code generator is running in strict mode.
        /// </summary>
        public bool StrictMode { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Visits an error generated through parsing.
        /// </summary>
        /// <param name="err">The error that was generated.</param>
        public override void VisitError(RazorError err)
        {
            if (this.StrictMode)
            {
                throw new TemplateParsingException(err);
            }
        }

        /// <summary>
        /// Visits any specialised spans used to introduce custom logic.
        /// </summary>
        /// <param name="span">The current span.</param>
        /// <returns>True if additional spans were matched, otherwise false.</returns>
        protected override bool TryVisitSpecialSpan(Span span)
        {
            return TryVisit<ModelSpan>(span, this.VisitModelSpan);
        }

        /// <summary>
        /// Sets the model type.
        /// </summary>
        /// <param name="modelTypeName">The model type.</param>
        private void SetModelType(string modelTypeName)
        {
            var host = (Compilation.RazorEngineHost)Host;
            var type = host.DefaultBaseTemplateType;
            var className = type.Name;

            if (className.Contains("`"))
            {
                className = className.Substring(0, className.IndexOf('`'));
            }

            string baseName = string.Format("{0}.{1}(Of {2})", type.Namespace, className, modelTypeName);
            var baseType = new CodeTypeReference(baseName);
            GeneratedClass.BaseTypes.Clear();
            GeneratedClass.BaseTypes.Add(baseType);
        }

        /// <summary>
        /// Visits a model span.
        /// </summary>
        /// <param name="span">The model span.</param>
        private void VisitModelSpan(ModelSpan span)
        {
            this.SetModelType(span.ModelTypeName);

            if (DesignTimeMode)
            {
                WriteHelperVariable(span.Content, "__modelHelper");
            }
        }

        #endregion
    }
}