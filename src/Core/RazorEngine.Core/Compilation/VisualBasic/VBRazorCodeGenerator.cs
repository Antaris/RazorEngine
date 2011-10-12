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
        /// Sets the model type.
        /// </summary>
        /// <param name="modelTypeName">The model type.</param>
        private void SetModelType(string modelTypeName)
        {
            var host = (Compilation.RazorEngineHost)Host;
            var type = host.DefaultBaseTemplateType;

            string baseName = string.Format("{0}.{1}(Of {2})", type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), modelTypeName);
            var baseType = new CodeTypeReference(baseName);
            GeneratedClass.BaseTypes.Clear();
            GeneratedClass.BaseTypes.Add(baseType);
        }

        /// <summary>
        /// Visits any specialised spans used to introduce custom logic.
        /// </summary>
        /// <param name="span">The current span.</param>
        /// <returns>True if additional spans were matched, otherwise false.</returns>
        protected override bool TryVisitSpecialSpan(Span span)
        {
            return TryVisit<ModelSpan>(span, VisitModelSpan);
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

        /// <summary>
        /// Visits a model span.
        /// </summary>
        /// <param name="span">The model span.</param>
        private void VisitModelSpan(ModelSpan span)
        {
            SetModelType(span.ModelTypeName);

            if (DesignTimeMode)
                WriteHelperVariable(span.Content, "__modelHelper");
        }
        #endregion
    }
}