namespace RazorEngine.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
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
        private readonly CodeDomProvider _codeDomProvider;
        private bool _disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="DirectCompilerServiceBase"/>.
        /// </summary>
        /// <param name="codeLanguage">The razor code language.</param>
        /// <param name="codeDomProvider">The code dom provider used to generate code.</param>
        /// <param name="markupParserFactory">The markup parser factory.</param>
        protected DirectCompilerServiceBase(RazorCodeLanguage codeLanguage, CodeDomProvider codeDomProvider, Func<ParserBase> markupParserFactory)
            : base(codeLanguage, markupParserFactory)
        {
            _codeDomProvider = codeDomProvider;
        }
        #endregion
        
        #region Methods
        public static string GetTemporaryDirectory()
        {
            var created = false;
            var tried = 0;
            string tempDirectory = "";
            while (!created && tried < 10)
            {
                tried++;
                try
                {
                    tempDirectory = Path.Combine(Path.GetTempPath(), "RazorEngine_" + Path.GetRandomFileName());
                    if (!Directory.Exists(tempDirectory))
                    {
                        Directory.CreateDirectory(tempDirectory);
                        created = Directory.Exists(tempDirectory);
                    }
                }
                catch (IOException)
                {
                    if (tried > 8)
                    {
                        throw;
                    }
                }
            }
            if (!created)
            {
                throw new Exception("Could not create a temporary directory! Maybe all names are already used?");
            }
            return tempDirectory;
        }

        /// <summary>
        /// Creates the compile results for the specified <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="context">The type context.</param>
        /// <returns>The compiler results.</returns>
        [Pure]
        private Tuple<CompilerResults, string> Compile(TypeContext context)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,
                                                 context.TemplateType, context.ModelType);

            var @params = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                IncludeDebugInformation = Debug,
                TreatWarningsAsErrors = false,
                CompilerOptions = "/target:library /optimize /define:RAZORENGINE"
            };

            var assemblies = ReferenceResolver.GetReferences(context, IncludeAssemblies());

            assemblies = assemblies
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

            @params.ReferencedAssemblies.AddRange(assemblies.ToArray());

            string sourceCode = null;
            if (Debug)
            {
                @params.GenerateInMemory = false;
                @params.TempFiles = new TempFileCollection(GetTemporaryDirectory(), true);
                @params.IncludeDebugInformation = true;
                var builder = new StringBuilder();
                using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
                {
                    _codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, new CodeGeneratorOptions());
                    sourceCode = builder.ToString();
                }
            }
            
            var results = _codeDomProvider.CompileAssemblyFromDom(@params, compileUnit);
            if (Debug)
            {
                bool written = false;
                var targetFile = Path.Combine(results.TempFiles.TempDir, "generated_template." + SourceFileExtension);
                if (!written && !File.Exists(targetFile))
                {
                    File.WriteAllText(targetFile, sourceCode);
                    written = true;
                }
                if (!written)
                {
                    foreach (string item in results.TempFiles)
	                {
                        if (item.EndsWith("." + this.SourceFileExtension))
                        {
                            File.Copy(item, targetFile, true);
                            written = true;
                            break;
                        }
	                } 
                }
            }

            return Tuple.Create(results, sourceCode);
        }

        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        [Pure, SecurityCritical]
        public override Tuple<Type, CompilationData> CompileType(TypeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var result = Compile(context);
            var compileResult = result.Item1;

            CompilationData tmpDir = new CompilationData(result.Item2, null);
            if (compileResult.TempFiles != null)
            {
                tmpDir = new CompilationData(result.Item2, compileResult.TempFiles.TempDir);
            }
            if (compileResult.Errors != null && compileResult.Errors.HasErrors)
            {
                throw new TemplateCompilationException(compileResult.Errors, tmpDir, context.TemplateContent);
            }

            return Tuple.Create(
                compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
                tmpDir);
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
            if (disposing && !_disposed)
            {
                _codeDomProvider.Dispose();
                _disposed = true;
            }
        }
        #endregion
    }
}