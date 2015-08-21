using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
    interface ICodeGenerator
    {
        string GenerateCode();
    }

    public class BaseCodeGenerator : ICodeGenerator
    {
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

        protected internal BaseCodeGenerator(RazorCodeLanguage codeLanguage, ParserBaseCreator markupParserFactory)
        {
            _codeLanguage = codeLanguage;
            _markupParserFactory = markupParserFactory ?? new ParserBaseCreator(null);
        }
        
        public virtual string GenerateCode(string className, )
        {
            string className = context.ClassName;
            ITemplateSource template = context.TemplateContent;
            ISet<string> namespaceImports = context.Namespaces;
            Type templateType = context.TemplateType;
            Type modelType = context.ModelType;

            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("Class name is required.");

            if (template == null)
                throw new ArgumentException("Template is required.");

            namespaceImports = namespaceImports ?? new HashSet<string>();
            templateType = templateType ?? ((modelType == null) ? typeof(TemplateBase) : typeof(TemplateBase<>));

            // Create the RazorEngineHost
            var host = CreateHost(templateType, modelType, className);

            // Add any required namespace imports
            foreach (string ns in GetNamespaces(templateType, namespaceImports))
                host.NamespaceImports.Add(ns);

            // Gets the generator result.
            return GetGeneratorResult(host, context);
        }

        /// <summary>
        /// Creates a <see cref="RazorEngineHost"/> used for class generation.
        /// </summary>
        /// <param name="templateType">The template base type.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="className">The class name.</param>
        /// <returns>An instance of <see cref="RazorEngineHost"/>.</returns>

        [SecurityCritical]
        protected internal Compilation.RazorEngineHost CreateHost(Type templateType, Type modelType, string className)
        {
            var host = new Compilation.RazorEngineHost(_codeLanguage, _markupParserFactory.Create)
            {
                DefaultBaseTemplateType = templateType,
                DefaultModelType = modelType,
                DefaultBaseClass = BuildTypeName(templateType, modelType),
                DefaultClassName = className,
                DefaultNamespace = DynamicTemplateNamespace,
                GeneratedClassContext = new GeneratedClassContext("Execute", "Write", "WriteLiteral",
                                                                                 "WriteTo", "WriteLiteralTo",
                                                                                 "RazorEngine.Templating.TemplateWriter",
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

    public class CSharpCodeGenerator : ICodeGenerator
    {
        private CSharpCodeGenerator()
        {

        }

        string ICodeGenerator.GenerateCode()
        {
            return "";
        }
    }
}
