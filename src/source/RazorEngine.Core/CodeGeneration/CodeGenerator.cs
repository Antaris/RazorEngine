using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Templating;
using System.Diagnostics.Contracts;
using System.IO;
using System.Globalization;
using Microsoft.CSharp;
#if RAZOR4
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.CodeGenerators;
using Microsoft.AspNet.Razor.Chunks.Generators;
using Microsoft.AspNet.Razor.Parser;
#else
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.CodeDom.Compiler;
#endif
using RazorEngine.Compilation;


namespace RazorEngine.CodeGeneration
{
    public class RazorTemplaceGeneratedCode
    {
        private readonly string className;
        private readonly string classNamespace;
        private readonly string sourceCode;
        private readonly string sourceCodeLanguage;

        public RazorTemplaceGeneratedCode(string sourceCode, string sourceCodeLanguage, string className, string classNamespace)
        {
            this.sourceCode = sourceCode;
            this.sourceCodeLanguage = sourceCodeLanguage;
            this.className = className;
            this.classNamespace = classNamespace;
        }

        public string ClassName
        {
            get
            {
                return className;
            }
        }

        public string ClassNamespace
        {
            get
            {
                return classNamespace;
            }
        }

        public string SourceCode
        {
            get
            {
                return sourceCode;
            }
        }

        public string SourceCodeLanguage
        {
            get
            {
                return sourceCodeLanguage;
            }
        }
    }

    /// <summary>
    /// Represents a code generator, generating code of a specific language for a given template.
    /// </summary>
    interface ICodeGenerator
    {
        /// <summary>
        /// Executes Razor and generates code for the given templates
        /// </summary>
        /// <param name="className">the name of the generated class</param>
        /// <param name="classNamespace">the namespace of the generated class</param>
        /// <param name="template">the template to generate</param>
        /// <param name="namespaces">the namespaces to open</param>
        /// <param name="templateType">the type of the template (ie the base class)</param>
        /// <param name="modelType">the model type (if templateType is a generic type)</param>
        /// <returns></returns>
        RazorTemplaceGeneratedCode GenerateCode(string className, string classNamespace, ITemplateSource template, ISet<string> namespaces, Type templateType, Type modelType);
    }

    public abstract class BaseCodeGenerator : ICodeGenerator
    {
        /// <summary>
        /// The namespace for dynamic templates.
        /// </summary>
        protected internal const string DynamicTemplateNamespace = "CompiledRazorTemplates.Dynamic";

        /// <summary>
        /// A prefix for all dynamically created classes.
        /// </summary>
        protected internal const string ClassNamePrefix = "RazorEngine_";

        private ParserBaseCreator _markupParserFactory;
        private RazorCodeLanguage _codeLanguage;

        /// <summary>
        /// This class only exists because we cannot use Func&lt;ParserBase&gt; in non security-critical class.
        /// </summary>
        [SecurityCritical]
        public class ParserBaseCreator
        {
            /// <summary>
            /// The parser creator.
            /// </summary>
            private Func<ParserBase> creator;
            /// <summary>
            /// Create a new ParserBaseCreator instance.
            /// </summary>
            /// <param name="creator">The parser creator.</param>
            public ParserBaseCreator(Func<ParserBase> creator)
            {
                this.creator = creator ?? (() => new HtmlMarkupParser());
            }
            /// <summary>
            /// Execute the given delegate.
            /// </summary>
            /// <returns></returns>
            public ParserBase Create()
            {
                return this.creator();
            }
        }

        [SecurityCritical]
        protected BaseCodeGenerator(RazorCodeLanguage codeLanguage, ParserBaseCreator markupParserFactory)
        {
            _codeLanguage = codeLanguage;
            _markupParserFactory = markupParserFactory ?? new ParserBaseCreator(null);
        }

        internal RazorCodeLanguage CodeLanguage { [SecurityCritical] get { return _codeLanguage; } }

        internal Func<ParserBase> MarkupParserFactory { [SecurityCritical] get { return _markupParserFactory.Create; } }


        /// <summary>
        /// Gets any required namespace imports.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <param name="otherNamespaces">The requested set of namespace imports.</param>
        /// <returns>A set of namespace imports.</returns>
        internal static IEnumerable<string> GetNamespaces(Type templateType, IEnumerable<string> otherNamespaces)
        {
            var templateNamespaces = templateType.GetCustomAttributes(typeof(RequireNamespacesAttribute), true)
                .Cast<RequireNamespacesAttribute>()
                .SelectMany(a => a.Namespaces)
                .Concat(otherNamespaces)
                .Distinct();

            return templateNamespaces;
        }

        public virtual string EditCode(string code, string className, string classNamespace, ITemplateSource template, ISet<string> namespaces, Type templateType, Type modelType)
        {
            // Add the dynamic model attribute if the type is an anonymous type.
            if (modelType != null && CompilerServicesUtility.IsDynamicType(modelType))
            {
                // TODO: add HasDynamicModelAttribute to the generated type
                //generatedType.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HasDynamicModelAttribute))));
            }

            // Generate any constructors required by the base template type.
            var constructors = CompilerServicesUtility.GetConstructors(templateType);

            if (constructors == null || !constructors.Any())
                return code;

            // Remove all existing ones.
            //var existingConstructors = codeType.Members.OfType<CodeConstructor>().ToArray();
            //foreach (var existingConstructor in existingConstructors)
            //    codeType.Members.Remove(existingConstructor);

            foreach (var constructor in constructors)
            {
                foreach (var param in constructor.GetParameters())
                {
                    throw new InvalidOperationException("Custom contructors are currently not supported!");
                }
                //var ctor = new CodeConstructor();
                //ctor.Attributes = MemberAttributes.Public;
                //
                //foreach (var param in constructor.GetParameters())
                //{
                //    ctor.Parameters.Add(new CodeParameterDeclarationExpression(param.ParameterType, param.Name));
                //    ctor.BaseConstructorArgs.Add(new CodeSnippetExpression(param.Name));
                //}
                //
                //codeType.Members.Add(ctor);
            }

            return code;
        }

        [SecuritySafeCritical]
        public virtual RazorTemplaceGeneratedCode GenerateCode(string className, string classNamespace, ITemplateSource template, ISet<string> namespaces, Type templateType, Type modelType)
        {
            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("Class name is required.");

            if (template == null)
                throw new ArgumentException("Template is required.");

            namespaces = namespaces ?? new HashSet<string>();
            templateType = templateType ?? ((modelType == null) ? typeof(TemplateBase) : typeof(TemplateBase<>));

            // Create the RazorEngineHost
            var host = CreateHost(templateType, modelType, className, classNamespace);

            // Add any required namespace imports
            foreach (string ns in GetNamespaces(templateType, namespaces))
                host.NamespaceImports.Add(ns);

            // Gets the generator result.
            var sourceCode = GetGeneratorResult(host, template);

            // Improve debugging when no file location is given
            if (template.TemplateFile == null)
            {
                sourceCode = sourceCode.Replace("#line hidden", "");
            }

            sourceCode = EditCode(sourceCode, className, classNamespace, template, namespaces, templateType, modelType);

            return new RazorTemplaceGeneratedCode(sourceCode, _codeLanguage.LanguageName, className, classNamespace);
        }

#if !RAZOR4
        /// <summary>
        /// The CodeDomProvider for the current language.
        /// </summary>
        protected abstract CodeDomProvider CodeDomProvider { get; }
#endif
        /// <summary>
        /// Gets the generator result.
        /// </summary>
        /// <param name="host">The razor engine host.</param>
        /// <param name="template">The template.</param>
        /// <returns>The generator result.</returns>
        [SecurityCritical]
        protected string GetGeneratorResult(Compilation.RazorEngineHost host, ITemplateSource template)
        {
            var engine = new RazorTemplateEngine(host);
            GeneratorResults result;
            using (var reader = template.GetTemplateReader())
                result = engine.GenerateCode(reader, null, null, template.TemplateFile);
#if RAZOR4
            return result.GeneratedCode;
#else
            string generatedCode;
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                CodeDomProvider.GenerateCodeFromCompileUnit(result.GeneratedCode, writer, new CodeGeneratorOptions());
                generatedCode = builder.ToString();
            }
            return generatedCode;
#endif
        }

        /// <summary>
        /// Builds a type name for the specified template type.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>The string type name (including namespace).</returns>
        [Pure]
        protected abstract string BuildTypeName(Type templateType, Type modelType);

        /// <summary>
        /// Creates a <see cref="RazorEngineHost"/> used for class generation.
        /// </summary>
        /// <param name="templateType">The template base type.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="className">The class name.</param>
        /// <param name="classNamespace">the namespace of the class</param>
        /// <returns>An instance of <see cref="RazorEngineHost"/>.</returns>

        [SecurityCritical]
        protected internal Compilation.RazorEngineHost CreateHost(Type templateType, Type modelType, string className, string classNamespace)
        {
            var host = new Compilation.RazorEngineHost(_codeLanguage, _markupParserFactory.Create)
            {
                DefaultBaseTemplateType = templateType,
                DefaultModelType = modelType,
                DefaultBaseClass = BuildTypeName(templateType, modelType),
                DefaultClassName = className,
                DefaultNamespace = classNamespace,
                GeneratedClassContext = 
                    new GeneratedClassContext("Execute", "Write", "WriteLiteral",
                         "WriteTo", "WriteLiteralTo", "RazorEngine.Templating.TemplateWriter",
                         "DefineSection"
#if RAZOR4
                         , new GeneratedTagHelperContext()
#endif
                   )
                {
                    ResolveUrlMethodName = "ResolveUrl"
                }
            };

            return host;
        }
    }

}
