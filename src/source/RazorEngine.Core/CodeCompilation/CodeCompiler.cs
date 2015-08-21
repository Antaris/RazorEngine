using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.CodeCompilation
{
    interface ICodeCompiler
    {
        byte[] CompileCode(string code);
    }

    class CodeCompiler
    {
    }
}
