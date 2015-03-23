using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Permissions;

namespace RazorEngine.Compilation
{
    /// <summary>
    /// Provides (temporary) data about an compilation process.
    /// </summary>
    public class CompilationData : IDisposable
    {
        private bool _disposed;
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
        public CompilationData(string sourceCode, string tmpFolder)
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
        [SecuritySafeCritical]
        public void DeleteAll()
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (tmpFolder != null)
            {
                try
                {
                    foreach (var item in Directory.EnumerateFiles(tmpFolder))
                    {
                        try
                        {
                            File.Delete(item);
                        }
                        catch (IOException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                try
                {
                    foreach (var item in Directory.EnumerateDirectories(tmpFolder))
                    {
                        try
                        {
                            Directory.Delete(item, true);
                        }
                        catch (IOException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                try
                {
                    Directory.Delete(tmpFolder, true);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

            }
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
        /// Destructs the current instance.
        /// </summary>
        ~CompilationData()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clean up the compilation (ie delete temporary files).
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Cleans up the data of the current compilation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            DeleteAll();

            _disposed = true;
        }
    }
}
