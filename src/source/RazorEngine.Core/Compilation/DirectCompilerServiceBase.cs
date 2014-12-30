namespace RazorEngine.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
#if RAZOR4
    using Microsoft.AspNet.Razor;
    using Microsoft.AspNet.Razor.Parser;
#else
    using System.Web.Razor;
    using System.Web.Razor.Parser;
#endif
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
        [SecurityCritical]
        protected DirectCompilerServiceBase(RazorCodeLanguage codeLanguage, CodeDomProvider codeDomProvider, Func<ParserBase> markupParserFactory)
            : base(codeLanguage, new ParserBaseCreator(markupParserFactory))
        {
            _codeDomProvider = codeDomProvider;
        }
        #endregion

        #region Properties
#if !RAZOR4
        /// <summary>
        /// The underlaying CodeDomProvider instance.
        /// </summary>
        public virtual CodeDomProvider CodeDomProvider { get { return _codeDomProvider; } }
#endif
        #endregion

        #region Methods

        /// <summary>
        /// Creates the compile results for the specified <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="context">The type context.</param>
        /// <returns>The compiler results.</returns>
        [Pure]
        [SecurityCritical]
        private Tuple<CompilerResults, string> Compile(TypeContext context)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var sourceCode = GetCodeCompileUnit(context);

            var @params = new CompilerParameters
            {
                GenerateInMemory = false,
                GenerateExecutable = false,
                IncludeDebugInformation = Debug,
                TreatWarningsAsErrors = false,
                TempFiles = new TempFileCollection(GetTemporaryDirectory(), true),
                CompilerOptions = "/target:library /optimize /define:RAZORENGINE"
            };


            var assemblies = GetAllReferences(context);

            var fileAssemblies = assemblies
                .Select(a => a.GetFile(
                    msg => new ArgumentException(
                        string.Format(
                            "Unsupported CompilerReference given to CodeDom compiler (only references which can be resolved to files are supported: {0})!",
                            msg))))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.InvariantCultureIgnoreCase);

            @params.ReferencedAssemblies.AddRange(fileAssemblies.ToArray());
            var tempDir = @params.TempFiles.TempDir;
            var assemblyName = Path.Combine(tempDir,
                String.Format("{0}.dll", GetAssemblyName(context)));
            @params.TempFiles.AddFile(assemblyName, true);
            @params.OutputAssembly = assemblyName;
            
            var results = _codeDomProvider.CompileAssemblyFromSource(@params, new [] { sourceCode });
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
        /// Inspects the GeneratorResults and returns the source code.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [SecurityCritical]
        public override string InspectSource(GeneratorResults results, TypeContext context)
        {
#if RAZOR4
            string generatedCode = results.GeneratedCode;
            if (context.TemplateContent.TemplateFile == null)
            {
                generatedCode = generatedCode.Replace("#line hidden", "");
            }

            // TODO: add attributes and constructor to the source code.
            
            return generatedCode;
#else
            if (context.TemplateContent.TemplateFile == null) 
            {
                // Allow to step into the template code by removing the "#line hidden" pragmas
                foreach (CodeNamespace @namespace in results.GeneratedCode.Namespaces.Cast<CodeNamespace>().ToList())
                {
                    foreach (CodeTypeDeclaration @type in @namespace.Types.Cast<CodeTypeDeclaration>().ToList())
                    {
                        foreach (CodeTypeMember member in @type.Members.Cast<CodeTypeMember>().ToList())
                        {
                            var snippet = member as CodeSnippetTypeMember;
                            if (snippet != null && snippet.Text == "#line hidden")
                            {
                                @type.Members.Remove(snippet);
                            }
                        }
                    }
                }
            }

            
            // Add the dynamic model attribute if the type is an anonymous type.
            var generatedType = results.GeneratedCode.Namespaces[0].Types[0];
            if (context.ModelType != null && CompilerServicesUtility.IsDynamicType(context.ModelType))
                generatedType.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HasDynamicModelAttribute))));

            // Generate any constructors required by the base template type.
            GenerateConstructors(CompilerServicesUtility.GetConstructors(context.TemplateType), generatedType);

            // Despatch any inspectors
            Inspect(results.GeneratedCode);
            
            string generatedCode;
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                CodeDomProvider.GenerateCodeFromCompileUnit(results.GeneratedCode, writer, new CodeGeneratorOptions());
                generatedCode = builder.ToString();
            }
            return generatedCode;
#endif
        }

        /// <summary>
        /// Generates any required contructors for the specified type.
        /// </summary>
        /// <param name="constructors">The set of constructors.</param>
        /// <param name="codeType">The code type declaration.</param>
        [Pure]
        private static void GenerateConstructors(IEnumerable<ConstructorInfo> constructors, CodeTypeDeclaration codeType)
        {
            if (constructors == null || !constructors.Any())
                return;

            var existingConstructors = codeType.Members.OfType<CodeConstructor>().ToArray();
            foreach (var existingConstructor in existingConstructors)
                codeType.Members.Remove(existingConstructor);

            foreach (var constructor in constructors)
            {
                var ctor = new CodeConstructor();
                ctor.Attributes = MemberAttributes.Public;

                foreach (var param in constructor.GetParameters())
                {
                    ctor.Parameters.Add(new CodeParameterDeclarationExpression(param.ParameterType, param.Name));
                    ctor.BaseConstructorArgs.Add(new CodeSnippetExpression(param.Name));
                }

                codeType.Members.Add(ctor);
            }
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

            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            var result = Compile(context);
            var compileResult = result.Item1;

            CompilationData tmpDir;
            if (compileResult.TempFiles != null)
            {
                tmpDir = new CompilationData(result.Item2, compileResult.TempFiles.TempDir);
            }
            else
            {
                tmpDir = new CompilationData(result.Item2, null);
            }

            if (compileResult.Errors != null && compileResult.Errors.HasErrors)
            {
                throw new TemplateCompilationException(
                    compileResult.Errors
                    .Cast<CompilerError>()
                    .Select(error => 
                        new RazorEngineCompilerError(
                            error.ErrorText,
                            error.FileName,
                            error.Line,
                            error.Column,
                            error.ErrorNumber,
                            error.IsWarning)), 
                    tmpDir, context.TemplateContent);
            }
            // Make sure we load the assembly from a file and not with
            // Load(byte[]) (or it will be fully trusted!)
            var assemblyPath = compileResult.PathToAssembly;
            compileResult.CompiledAssembly = Assembly.LoadFile(assemblyPath);
            var type = compileResult.CompiledAssembly.GetType(DynamicTemplateNamespace + "." + context.ClassName);
            return Tuple.Create(type, tmpDir);
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