using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RazorEngine.Compilation.ReferenceResolver;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Manages the current list of assemblies, used for dynamic assembly resolution.
    /// When we compile assemblies we might have any <see cref="CompilerReference"/>, but once we load
    /// the template the runtime will search for it and trigger an <see cref="System.AppDomain.AssemblyLoad"/> event.
    /// We can handle the event by searching in the already used list of references, which is managed by this class.
    /// </summary>
    internal class ReferencesListForDynamicAssemblyResolution : IDisposable
    {
        /// <summary>
        /// All references we used until now.
        /// </summary>
        private readonly HashSet<CompilerReference> _references = new HashSet<CompilerReference>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Add references to the current list of compiler references.
        /// This member is threadsafe.
        /// </summary>
        /// <param name="refs">The compiler references to add.</param>
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
                if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get the current set of <see cref="CompilerReference"/> instances.
        /// Note that this method returnes a copied snapshot and is therefore threadsafe.
        /// An other thread might add additional <see cref="CompilerReference"/> objects while we enumerate the list.
        /// But that should not matter as the <see cref="System.AppDomain.AssemblyLoad"/> event was triggered earlier.
        /// </summary>
        /// <returns>the current list of compiler references.</returns>
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

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}