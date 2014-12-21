using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation
{/// <summary>
    /// Enables access to objects across application domain boundaries.
    /// This type differs from <see cref="MarshalByRefObject"/> by ensuring that the
    /// service lifetime is managed deterministically by the consumer.
    /// </summary>
    public abstract class CrossAppDomainObject : MarshalByRefObject, IDisposable
    {
        private bool _disposed;

        ~CrossAppDomainObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disconnects the remoting channel(s) of this object and all nested objects.
        /// </summary>
        private void Disconnect()
        {
            RemotingServices.Disconnect(this);
        }

        public sealed override object InitializeLifetimeService()
        {
            //
            // Returning null designates an infinite non-expiring lease.
            // We must therefore ensure that RemotingServices.Disconnect() is called when
            // it's no longer needed otherwise there will be a memory leak.
            //
            return null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Disconnect();
            _disposed = true;
        }

    }
}
