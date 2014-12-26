using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine
{
    /// <summary>
    /// Enables access to objects across application domain boundaries.
    /// This type differs from <see cref="MarshalByRefObject"/> by ensuring that the
    /// service lifetime is managed deterministically by the consumer.
    /// </summary>
    public abstract class CrossAppDomainObject : MarshalByRefObject, IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// Cleans up the <see cref="CrossAppDomainObject"/> instance.
        /// </summary>
        ~CrossAppDomainObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disconnects the remoting channel(s) of this object and all nested objects.
        /// </summary>
        [SecuritySafeCritical]
        private void Disconnect()
        {
            RemotingServices.Disconnect(this);
        }

        /// <summary>
        /// initializes the lifetime service for the current instance.
        /// </summary>
        /// <returns>null</returns>
        [SecurityCritical]
        public sealed override object InitializeLifetimeService()
        {
            //
            // Returning null designates an infinite non-expiring lease.
            // We must therefore ensure that RemotingServices.Disconnect() is called when
            // it's no longer needed otherwise there will be a memory leak.
            //
            return null;
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Disposes the current instance via the disposable pattern.
        /// </summary>
        /// <param name="disposing">true when Dispose() was called manually.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Disconnect();
            _disposed = true;
        }

    }
}
