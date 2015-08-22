using Microsoft.CSharp;
using Microsoft.VisualBasic;
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

namespace RazorEngine.CodeGeneration.VisualBasic
{
    public class VisualBasicCodeGenerator : BaseCodeGenerator
    {
        private readonly VBCodeProvider _codeDomProvider;

        [SecurityCritical]
        public VisualBasicCodeGenerator(bool strictMode = false, Func<ParserBase> markupParserFactory = null)
            : base(new Compilation.VisualBasic.VBRazorCodeLanguage(strictMode), new ParserBaseCreator(markupParserFactory))
        {
            _codeDomProvider = new VBCodeProvider();
        }

        protected override CodeDomProvider CodeDomProvider
        {
            get
            {
                return _codeDomProvider;
            }
        }

        protected override string BuildTypeName(Type templateType, Type modelType)
        {
            if (templateType == null)
                throw new ArgumentNullException("templateType");

            var modelTypeName = CompilerServicesUtility.ResolveVBTypeName(modelType);
            return CompilerServicesUtility.VBCreateGenericType(templateType, modelTypeName, false);
        }
    }
}
