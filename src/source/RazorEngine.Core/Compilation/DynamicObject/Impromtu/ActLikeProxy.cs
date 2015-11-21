// 
//  Copyright 2010  Ekon Benefits
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using RazorEngine.Compilation.ImpromptuInterface;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
//using RazorEngine.Compilation.ImpromptuInterface.Internal.Support;
using RazorEngine.Compilation.ImpromptuInterface.Optimization;
using System.Security;

namespace RazorEngine.Compilation.ImpromptuInterface.Build
{
    /// <summary>
    /// This interface can be used to define your own custom proxy if you preload it.
    /// </summary>
    /// <remarks>
    /// Advanced usage only! This is required as well as <see cref="ActLikeProxyAttribute"></see>
    /// </remarks>
    public interface IActLikeProxyInitialize : IActLikeProxy
    {
        ///<summary>
        /// Method used to Initialize Proxy
        ///</summary>
        ///<param name="original"></param>
        ///<param name="interfaces"></param>
        ///<param name="informalInterface"></param>
        void Initialize(dynamic original, IEnumerable<Type> interfaces = null, IDictionary<string, Type> informalInterface = null);
    }


    /// <summary>
    /// Base class of Emited ProxiesC:\Documents and Settings\jayt\My Documents\Visual Studio 2010\Projects\RazorEngine.Compilation.ImpromptuInterface\RazorEngine.Compilation.ImpromptuInterface\Optimization\
    /// </summary>
    [Serializable]
    public abstract class ActLikeProxy : ImpromptuForwarder, IActLikeProxyInitialize, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActLikeProxy"/> class.
        /// </summary>
        public ActLikeProxy() : base(null)
        {

        }
        /// <summary>
        /// Returns the proxied object
        /// </summary>
        /// <value></value>
        private dynamic ActLikeProxyOriginal { get; set; }

        dynamic IActLikeProxy.Original { get { return ActLikeProxyOriginal; } }

        private bool _init = false;

        /// <summary>
        /// Method used to Initialize Proxy
        /// </summary>
        /// <param name="original"></param>
        /// <param name="interfaces"></param>
        /// <param name="informalInterface"></param>
        void IActLikeProxyInitialize.Initialize(dynamic original, IEnumerable<Type> interfaces, IDictionary<string, Type> informalInterface)
        {
            if (((object)original) == null)
                throw new ArgumentNullException("original", "Can't proxy a Null value");

            if (_init)
                throw new MethodAccessException("Initialize should not be called twice!");
            _init = true;
            ActLikeProxyOriginal = original;
            Target = original;

            //Let IDynamicKnowLike know about interfaces
            /* will be deprecated in the future */
            var tKnowOriginal = ActLikeProxyOriginal as IDynamicKnowLike;
            if (tKnowOriginal != null)
            {
                if (interfaces != null)
                    tKnowOriginal.KnownInterfaces = interfaces;
                if (informalInterface != null)
                    tKnowOriginal.KnownPropertySpec = informalInterface;
            }


            //Uses standard lib component types to notify proxied object of interfaces.
            var serviceProvider = ActLikeProxyOriginal as IServiceProvider;
            if (serviceProvider != null)
            {
                try
                {
                    serviceProvider.GetService(GetType());
                }
                catch
                {

                }
            }
        }



        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(ActLikeProxyOriginal, obj)) return true;
            if (!(obj is ActLikeProxy)) return ActLikeProxyOriginal.Equals(obj);
            return Equals((ActLikeProxy)obj);
        }

        /// <summary>
        /// Actlike proxy should be equivalent to the objects they proxy
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(ActLikeProxy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(ActLikeProxyOriginal, other.ActLikeProxyOriginal)) return true;
            return Equals(other.ActLikeProxyOriginal, ActLikeProxyOriginal);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ActLikeProxyOriginal.GetHashCode();
        }

#if !SILVERLIGHT

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(ActLikeProxySerializationHelper));

            var tCustomAttr =
                GetType().GetCustomAttributes(typeof(ActLikeProxyAttribute), false).OfType<ActLikeProxyAttribute>().
                    FirstOrDefault();


            info.AddValue("Context",
                          tCustomAttr == null
                          ? null
                          : tCustomAttr.Context, typeof(Type));


            info.AddValue("Interfaces",
                      tCustomAttr == null
                      ? null
                      : tCustomAttr.Interfaces, typeof(Type[]));

            info.AddValue("Original", (object)ActLikeProxyOriginal);

        }
#endif

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ActLikeProxyOriginal.ToString();
        }
    }
}
