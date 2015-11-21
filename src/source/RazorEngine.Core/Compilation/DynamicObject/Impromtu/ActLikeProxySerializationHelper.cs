using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace RazorEngine.Compilation.ImpromptuInterface.Build
{
#if !SILVERLIGHT



    /// <summary>
    /// Support Deserializing the proxy since on separate runs of an executable
    /// </summary>
    [Serializable]
    public class ActLikeProxySerializationHelper : IObjectReference
    {
        /// <summary>
        /// Original Object
        /// </summary>
        public object Original;
        /// <summary>
        /// Intefaces
        /// </summary>
        public Type[] Interfaces;

        /// <summary>
        /// Type Context
        /// </summary>
        public Type Context;

        /// <summary>
        /// Returns the real object that should be deserialized, rather than the object that the serialized stream specifies.
        /// </summary>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> from which the current object is deserialized.</param>
        /// <returns>
        /// Returns the actual object that is put into the graph.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. The call will not work on a medium trusted server.</exception>
        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            var tInterfaces = Interfaces;
            if (tInterfaces == null)
            {
                throw new InvalidOperationException("Expected SerializationData to contain a non-null Interfaces field. Please consider upgrading mono!");
            }

            var tType = BuildProxy.BuildType(Context, tInterfaces.First(), tInterfaces.Skip(1).ToArray());
            return Impromptu.InitializeProxy(tType, Original, tInterfaces);
        }

    }
#endif
}
