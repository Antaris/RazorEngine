using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation
{
    /// <summary>
    /// Provides (temporary) data about an compilation process.
    /// </summary>
    public class CompilationData : IDisposable
    {
        /// <summary>
        /// The temporary folder for the compilation process
        /// </summary>
        private string tmpFolder;
        /// <summary>
        /// The generated source code for the template.
        /// </summary>
        private string srcCode;
        /// <summary>
        /// Creates a new CompilationData instance.
        /// </summary>
        /// <param name="sourceCode">The generated source code for the template.</param>
        /// <param name="tmpFolder">The temporary folder for the compilation process</param>
        internal CompilationData(string sourceCode, string tmpFolder)
        {
            this.tmpFolder = tmpFolder;
            this.srcCode = sourceCode;
        }

        /// <summary>
        /// The generated source code of the template (can be null).
        /// </summary>
        public string SourceCode
        {
            get
            {
                return srcCode;
            }
        }

        /// <summary>
        /// Deletes all remaining files
        /// </summary>
        public void DeleteAll()
        {
        }

        /// <summary>
        /// returns the temporary folder for the compilation process (can be null).
        /// </summary>
        internal string TmpFolder
        {
            get
            {
                return tmpFolder;
            }
        }

        /// <summary>
        /// Clean up the compilation (ie delete temporary files).
        /// </summary>
        public void Dispose()
        {
            DeleteAll();
        }
    }
}
