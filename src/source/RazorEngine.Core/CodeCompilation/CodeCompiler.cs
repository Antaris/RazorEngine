using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.CodeCompilation
{
    public interface ICodeCompiler
    {
        byte[] CompileCode(string code);
    }

    public abstract class CodeCompilerBase : ICodeCompiler
    {

        public abstract byte[] CompileCode(string code);
    }

    public class CodeDomCodeCompilerBase : ICodeCompiler
    {
        private CodeDomProvider _codeDomProvider;

        public CodeDomCodeCompilerBase(CodeDomProvider codeDomProvider)
        {
            _codeDomProvider = codeDomProvider;
        }
        
        public virtual void EditParameters (CompilerParameters parameters)
        {
        }

        public byte[] CompileCode(string code)
        {
            var parameter = new CompilerParameters();
            //parameter.GenerateInMemory = true;
            parameter.IncludeDebugInformation = true;
            EditParameters(parameter);

            var results = _codeDomProvider.CompileAssemblyFromSource(parameter, code);
            var assemblyPath = results.PathToAssembly;
            var contents = File.ReadAllBytes(assemblyPath);
            results.TempFiles.Delete();
            return contents;
        }
    }
}
