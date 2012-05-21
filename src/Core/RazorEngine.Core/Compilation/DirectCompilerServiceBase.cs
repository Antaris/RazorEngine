//-----------------------------------------------------------------------------
// <copyright file="DirectCompilerServiceBase.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
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

        /// <summary>
        /// The Code Dom provider
        /// </summary>
        private readonly CodeDomProvider codeDomProvider;

        /// <summary>
        /// The flag
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectCompilerServiceBase"/> class.
        /// </summary>
        /// <param name="codeLanguage">The razor code language.</param>
        /// <param name="codeDomProvider">The code dom provider used to generate code.</param>
        /// <param name="markupParserFactory">The markup parser factory.</param>
        protected DirectCompilerServiceBase(RazorCodeLanguage codeLanguage, CodeDomProvider codeDomProvider, Func<MarkupParser> markupParserFactory)
            : base(codeLanguage, markupParserFactory)
        {
            this.codeDomProvider = codeDomProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        [Pure, SecurityCritical]
        public override Tuple<Type, Assembly> CompileType(TypeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var result = this.Compile(context);
            var compileResult = result.Item1;

            if (compileResult.Errors != null && compileResult.Errors.Count > 0)
            {
                throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);
            }

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
        /// <param name="disposing">Are we explicitly disposing of this instance?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !this.isDisposed)
            {
                this.codeDomProvider.Dispose();
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Creates the compile results for the specified <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="context">The type context.</param>
        /// <returns>The compiler results.</returns>
        [Pure]
        private Tuple<CompilerResults, string> Compile(TypeContext context)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var compileUnit = GetCodeCompileUnit(
                context.ClassName,
                context.TemplateContent,
                context.Namespaces,
                context.TemplateType,
                context.ModelType);

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

            var includeAssemblies = IncludeAssemblies() ?? Enumerable.Empty<string>();
            assemblies = assemblies.Concat(includeAssemblies)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

            @params.ReferencedAssemblies.AddRange(assemblies.ToArray());

            string sourceCode = null;
            if (Debug)
            {
                var builder = new StringBuilder();
                using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
                {
                    this.codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
                    sourceCode = builder.ToString();
                }
            }

            return Tuple.Create(this.codeDomProvider.CompileAssemblyFromDom(@params, compileUnit), sourceCode);
        }

        #endregion
    }
}