using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation
{
    public class CompilationData : IDisposable
    {
        private string tmpFolder;
        private string srcCode;
        internal CompilationData(string sourceCode, string tmpFolder)
        {
            this.tmpFolder = tmpFolder;
            this.srcCode = sourceCode;
        }

        public string SourceCode
        {
            get
            {
                return srcCode;
            }
        }

        public void DeleteAll()
        {
        }

        internal string TmpFolder
        {
            get
            {
                return tmpFolder;
            }
        }

        public void Dispose()
        {
            DeleteAll();
        }
    }
}
