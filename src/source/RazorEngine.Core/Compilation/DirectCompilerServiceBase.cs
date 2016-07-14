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
    using Microsoft.AspNetCore.Razor;
    using Microsoft.AspNetCore.Razor.CodeGenerators;
    using Microsoft.AspNetCore.Razor.Parser;
#else
    using System.Web.Razor;
    using System.Web.Razor.Parser;
#endif
    using Templating;
    using System.Security.Principal;

    /// <summary>
    /// Provides a base implementation of a direct compiler service.
    /// </summary>
    public abstract class DirectCompilerServiceBase : CompilerServiceBase
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
            var assemblies = GetAllReferences(context);

            var fileAssemblies = assemblies
                .Select(a => a.GetFile(
                    msg => new ArgumentException(
                        string.Format(
                            "Unsupported CompilerReference given to CodeDom compiler (only references which can be resolved to files are supported: {0})!",
                            msg))))
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.InvariantCultureIgnoreCase)
                .ToArray();
            var haveMscorlib = fileAssemblies.Any(a => a.Contains("mscorlib.dll"));

            var @params = new CompilerParameters
            {
                GenerateInMemory = false,
                GenerateExecutable = false,
                IncludeDebugInformation = Debug,
                TreatWarningsAsErrors = false,
                TempFiles = new TempFileCollection(GetTemporaryDirectory(), true),
                CompilerOptions =
                    string.Format("/target:library /optimize /define:RAZORENGINE {0}",
                        haveMscorlib ? "/nostdlib" : "")
            };


            @params.ReferencedAssemblies.AddRange(fileAssemblies);
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
                            if (snippet != null && snippet.Text.Contains("#line hidden"))
                            {
                                snippet.Text = snippet.Text.Replace("#line hidden", "");
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

#pragma warning disable 0618 // Backwards Compat.
            // Despatch any inspectors
            Inspect(results.GeneratedCode);
#pragma warning restore 0618 // Backwards Compat.
            
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
        
        [Pure, SecurityCritical]
        private Tuple<Type, CompilationData> CompileTypeImpl(TypeContext context)
        {
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
            if (DisableTempFileLocking)
            {
                compileResult.CompiledAssembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
            }
            else
            {
                compileResult.CompiledAssembly = Assembly.LoadFile(assemblyPath);
            }
            var type = compileResult.CompiledAssembly.GetType(DynamicTemplateNamespace + "." + context.ClassName);
            if (type == null)
            {
                try
                {
                    compileResult.CompiledAssembly.GetTypes();
                }
                catch (Exception e)
                {
                    throw new TemplateLoadingException("Unable to load types of the laded assembly", e);
                }
                // if we are here we just throw
                throw new TemplateLoadingException("We could not find the type in the compiled assembly!");
            }
            return Tuple.Create(type, tmpDir);
        }

        
        [Pure, SecurityCritical]
        private Tuple<Type, CompilationData> CompileType_Windows(TypeContext context)
        {
            // NOTE: The static constructor of WindowsImpersonationContext can fail, 
            // that's why we need to do that in a seperate method
            // -> Static constructor will not run

            /* Exception details: (2015-01-23: https://travis-ci.org/Antaris/RazorEngine/builds/47985319)
              System.Security.SecurityException : Couldn't impersonate token.
              at System.Security.Principal.WindowsImpersonationContext..ctor (IntPtr token) [0x00000] in <filename unknown>:0
              at System.Security.Principal.WindowsIdentity.Impersonate (IntPtr userToken) [0x00000] in <filename unknown>:0
              at RazorEngine.Compilation.DirectCompilerServiceBase.CompileType (RazorEngine.Compilation.TypeContext context) [0x00033] in /home/travis/build/Antaris/RazorEngine/src/source/RazorEngine.Core/Compilation/DirectCompilerServiceBase.cs:276 
             */ 
            WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero);
            try
            {
                return CompileTypeImpl(context);
            }
            finally
            {
                wic.Undo();
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
            var isMono = Type.GetType("Mono.Runtime") != null;
            if (!isMono)
            {
                return CompileType_Windows(context);
            }
            else
            {
                return CompileTypeImpl(context);
            }
        }

        /// <summary>
        /// Releases managed resources used by this instance.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _codeDomProvider.Dispose();
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}