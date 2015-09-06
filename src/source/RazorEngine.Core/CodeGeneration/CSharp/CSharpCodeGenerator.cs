using Microsoft.CSharp;
using RazorEngine.Compilation;
using System;
using System.CodeDom.Compiler;
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
#endif

namespace RazorEngine.CodeGeneration.CSharp
{
    public class CSharpCodeGenerator : BaseCodeGenerator
    {
        private readonly CSharpCodeProvider _codeDomProvider;

        [SecurityCritical]
        public CSharpCodeGenerator(bool strictMode = false, Func<ParserBase> markupParserFactory = null)
            : base(new Compilation.CSharp.CSharpRazorCodeLanguage(strictMode), new ParserBaseCreator(markupParserFactory))
        {
            _codeDomProvider = new CSharpCodeProvider();
        }

#if !RAZOR4
        protected override CodeDomProvider CodeDomProvider
        {
            get
            {
                return _codeDomProvider;
            }
        }
#endif

        protected override string BuildTypeName(Type templateType, Type modelType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            var modelTypeName = CompilerServicesUtility.ResolveCSharpTypeName(modelType);
            return CompilerServicesUtility.CSharpCreateGenericType(templateType, modelTypeName, false);
        }
    }
}
