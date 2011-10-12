namespace RazorEngine.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Web.Razor;
    using System.Web.Razor.Parser;

    using Templating;

    /// <summary>
    /// Provides a base implementation of a direct compiler service.
    /// </summary>
    public abstract class DirectCompilerServiceBase : CompilerServiceBase, IDisposable
    {
        #region Fields
        private readonly CodeDomProvider CodeDomProvider;
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DirectCompilerServiceBase"/>.
        /// </summary>
        /// <param name="codeLanguage">The razor code language.</param>
        /// <param name="codeDomProvider">The code dom provider used to generate code.</param>
        /// <param name="markupParserFactory">The markup parser factory.</param>
        protected DirectCompilerServiceBase(RazorCodeLanguage codeLanguage, CodeDomProvider codeDomProvider, Func<MarkupParser> markupParserFactory)
            : base(codeLanguage, markupParserFactory)
        {
            CodeDomProvider = codeDomProvider;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates the compile results for the specified <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="context">The type context.</param>
        /// <returns>The compiler results.</returns>
        [Pure]
        private Tuple<CompilerResults, string> Compile(TypeContext context)
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,
                                                 context.TemplateType, context.ModelType);

            var @params = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                CompilerOptions = "/target:library /optimize"
            };

            var assemblies = CompilerServicesUtility
                .GetLoadedAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);

            var includeAssemblies = (IncludeAssemblies() ?? Enumerable.Empty<string>());
            assemblies = assemblies.Concat(includeAssemblies)
                .Select(a => a.ToUpperInvariant())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct();

            @params.ReferencedAssemblies.AddRange(assemblies.ToArray());

            string sourceCode = null;
            if (Debug)
            {
                var builder = new StringBuilder();
                using (var writer = new StringWriter(builder))
                {
                    CodeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
                    sourceCode = builder.ToString();
                }
            }

            return Tuple.Create(CodeDomProvider.CompileAssemblyFromDom(@params, compileUnit), sourceCode);
        }

        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        [Pure, SecurityCritical]
        public override Tuple<Type, Assembly> CompileType(TypeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var result = Compile(context);
            var compileResult = result.Item1;

            if (compileResult.Errors != null && compileResult.Errors.Count > 0)
                throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);

            return Tuple.Create(
                compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
                compileResult.CompiledAssembly);
        }

        /// <summary>
        /// Releases managed resourced used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing">Are we explicily disposing of this instance?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                CodeDomProvider.Dispose();
                disposed = true;
            }
        }
        #endregion
    }
}