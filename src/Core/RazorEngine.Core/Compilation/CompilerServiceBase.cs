namespace RazorEngine.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser;

    using Inspectors;
    using Templating;

    /// <summary>
    /// Provides a base implementation of a compiler service.
    /// </summary>
    public abstract class CompilerServiceBase : MarshalByRefObject, ICompilerService
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="CompilerServiceBase"/>
        /// </summary>
        /// <param name="codeLanguage">The code language.</param>
        /// <param name="markupParserFactory">The markup parser factory.</param>
        protected CompilerServiceBase(RazorCodeLanguage codeLanguage, Func<ParserBase> markupParserFactory)
        {
            Contract.Requires(codeLanguage != null);

            CodeLanguage = codeLanguage;
            MarkupParserFactory = markupParserFactory ?? (() => new HtmlMarkupParser());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the set of code inspectors.
        /// </summary>
        public IEnumerable<ICodeInspector> CodeInspectors { get; set; }

        /// <summary>
        /// Gets the code language.
        /// </summary>
        public RazorCodeLanguage CodeLanguage { get; private set; }

        /// <summary>
        /// Gets or sets whether the compiler service is operating in debug mode.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Gets the markup parser.
        /// </summary>
        public Func<ParserBase> MarkupParserFactory { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Builds a type name for the specified template type.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <returns>The string type name (including namespace).</returns>
        [Pure]
        public virtual string BuildTypeName(Type templateType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            if (!templateType.IsGenericTypeDefinition || !templateType.IsGenericType)
                return templateType.FullName;

            return templateType.Namespace
                   + "."
                   + templateType.Name.Substring(0, templateType.Name.IndexOf('`'));
        }

        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        public abstract Tuple<Type, Assembly> CompileType(TypeContext context);

        /// <summary>
        /// Creates a <see cref="RazorEngineHost"/> used for class generation.
        /// </summary>
        /// <param name="templateType">The template base type.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="className">The class name.</param>
        /// <returns>An instance of <see cref="RazorEngineHost"/>.</returns>
        private RazorEngineHost CreateHost(Type templateType, Type modelType, string className)
        {
            var host = new RazorEngineHost(CodeLanguage, MarkupParserFactory)
                           {
                               DefaultBaseTemplateType = templateType,
                               DefaultModelType = modelType,
                               DefaultBaseClass = BuildTypeName(templateType),
                               DefaultClassName = className,
                               DefaultNamespace = "CompiledRazorTemplates.Dynamic",
                               GeneratedClassContext = new GeneratedClassContext("Execute", "Write", "WriteLiteral",
                                                                                 "WriteTo", "WriteLiteralTo",
                                                                                 "RazorEngine.Templating.TemplateWriter",
                                                                                 "DefineSection") {
                                                                                     ResolveUrlMethodName = "ResolveUrl"
                                                                                 }
                           };

            return host;
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
        /// Gets the code compile unit used to compile a type.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <param name="template">The template to compile.</param>
        /// <param name="namespaceImports">The set of namespace imports.</param>
        /// <param name="templateType">The template type.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>A <see cref="CodeCompileUnit"/> used to compile a type.</returns>
        [Pure]
        public CodeCompileUnit GetCodeCompileUnit(string className, string template, ISet<string> namespaceImports, Type templateType, Type modelType)
        {
            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("Class name is required.");

            if (string.IsNullOrEmpty(template))
                throw new ArgumentException("Template is required.");

            namespaceImports = namespaceImports ?? new HashSet<string>();
            templateType = templateType ?? ((modelType == null) ? typeof(TemplateBase) : typeof(TemplateBase<>));

            // Create the RazorEngineHost
            var host = CreateHost(templateType, modelType, className);

            // Add any required namespace imports
            foreach (string ns in GetNamespaces(templateType, namespaceImports))
                host.NamespaceImports.Add(ns);

            // Gets the generator result.
            GeneratorResults result = GetGeneratorResult(host, template);

            // Add the dynamic model attribute if the type is an anonymous type.
            var type = result.GeneratedCode.Namespaces[0].Types[0];
            if (modelType != null && CompilerServicesUtility.IsAnonymousType(modelType))
                type.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HasDynamicModelAttribute))));

            // Generate any constructors required by the base template type.
            GenerateConstructors(CompilerServicesUtility.GetConstructors(templateType), type);

            // Despatch any inspectors
            Inspect(result.GeneratedCode);

            return result.GeneratedCode;
        }

        /// <summary>
        /// Gets the generator result.
        /// </summary>
        /// <param name="host">The razor engine host.</param>
        /// <param name="template">The template.</param>
        /// <returns>The generator result.</returns>
        private static GeneratorResults GetGeneratorResult(RazorEngineHost host, string template)
        {
            var engine = new RazorTemplateEngine(host);
            GeneratorResults result;
            using (var reader = new StringReader(template))
                result = engine.GenerateCode(reader);

            return result;
        }

        /// <summary>
        /// Gets any required namespace imports.
        /// </summary>
        /// <param name="templateType">The template type.</param>
        /// <param name="otherNamespaces">The requested set of namespace imports.</param>
        /// <returns>A set of namespace imports.</returns>
        private static IEnumerable<string> GetNamespaces(Type templateType, IEnumerable<string> otherNamespaces)
        {
            var templateNamespaces = templateType.GetCustomAttributes(typeof(RequireNamespacesAttribute), true)
                .Cast<RequireNamespacesAttribute>()
                .SelectMany(a => a.Namespaces)
                .Concat(otherNamespaces)
                .Distinct();

            return templateNamespaces;
        }

        /// <summary>
        /// Returns a set of assemblies that must be referenced by the compiled template.
        /// </summary>
        /// <returns>The set of assemblies.</returns>
        public virtual IEnumerable<string> IncludeAssemblies()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Inspects the generated code compile unit.
        /// </summary>
        /// <param name="unit">The code compile unit.</param>
        protected virtual void Inspect(CodeCompileUnit unit)
        {
            Contract.Requires(unit != null);

            var ns = unit.Namespaces[0];
            var type = ns.Types[0];
            var executeMethod = type.Members.OfType<CodeMemberMethod>().Where(m => m.Name.Equals("Execute")).Single();

            foreach (var inspector in CodeInspectors)
                inspector.Inspect(unit, ns, type, executeMethod);
        }
        #endregion
    }
}