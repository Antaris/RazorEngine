using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RazorEngine.Compilation.ReferenceResolver;

namespace RazorEngine.Templating
{
    internal class ReferencesListForDynamicAssemblyResolution
    {
        /// <summary>
        /// All references we used until now.
        /// </summary>
        private readonly HashSet<CompilerReference> _references = new HashSet<CompilerReference>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void AddReferences(IEnumerable<CompilerReference> refs)
        {
            _lock.EnterWriteLock();
            try
            {
                foreach (var compilerReference in refs)
                {
                    _references.Add(compilerReference);
                }
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }


        public IEnumerable<CompilerReference> GetCurrentReferences()
        {
            _lock.EnterReadLock();
            try
            {
                return _references.ToList();
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        } 
    }
}