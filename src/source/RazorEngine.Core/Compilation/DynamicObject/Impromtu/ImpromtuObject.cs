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
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using RazorEngine.Compilation.ImpromptuInterface.Build;
//using RazorEngine.Compilation.ImpromptuInterface.Internal.Support;
using System.Reflection;
using System.Security;
namespace RazorEngine.Compilation.ImpromptuInterface.Dynamic
{
    interface ICustomTypeProvider { }

    /// <summary>
    /// Dynamic Object that knows about the Impromtu Interface return types;
    /// Override Typical Dynamic Object methods, and use TypeForName to get the return type of an interface member.
    /// </summary>

    [Serializable]
    public abstract class ImpromptuObject : DynamicObject, IDynamicKnowLike, 
        IActLike, ISerializable, ICustomTypeProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuObject"/> class.
        /// </summary>
        protected ImpromptuObject()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuObject"/> class. when deserializing
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ImpromptuObject(SerializationInfo info,
           StreamingContext context)
        {

        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {

        }

        /// <summary>
        /// Cache to avoid refelection for same Interfaces.
        /// </summary>
        protected static readonly IDictionary<TypeHash, IDictionary<string, Type>> _returnTypHash =
        new Dictionary<TypeHash, IDictionary<string, Type>>();


        private static readonly object TypeHashLock = new object();
        /// <summary>
        /// Hash for this instance to lookup cached values from <see cref="_returnTypHash"/>
        /// </summary>
        protected TypeHash _hash;

        /// <summary>
        /// Keep Track of Known Property Spec
        /// </summary>
        protected IDictionary<string, Type> PropertySpec;


        IEnumerable<Type> IDynamicKnowLike.KnownInterfaces
        {
            set { KnownInterfaces = value; }
        }
        
        /// <summary>
        /// Gets or sets the known interfaces.
        /// Set should only be called be the factory methood
        /// </summary>
        /// <value>The known interfaces.</value>
        protected virtual IEnumerable<Type> KnownInterfaces
        {
            get
            {
                if (PropertySpec != null)
                    return Enumerable.Empty<Type>();

                if (_hash == null)
                {
                    return Enumerable.Empty<Type>();
                }

                return _hash.Types.Cast<Type>();
            }
            set
            {
                lock (TypeHashLock)
                {
                    PropertySpec = null;

                    _hash = TypeHash.Create(value);
                    if (_returnTypHash.ContainsKey(_hash)) return;

                    var tPropReturType = value.SelectMany(@interface => @interface.GetProperties())
                        .Where(property => property.GetGetMethod() != null)
                        .Select(property => new { property.Name, property.GetGetMethod().ReturnType });

                    //If type can be determined by name
                    var tMethodReturnType = value.SelectMany(@interface => @interface.GetMethods())
                      .Where(method => !method.IsSpecialName)
                      .GroupBy(method => method.Name)
                      .Where(group => group.Select(method => method.ReturnType).Distinct().Count() == 1)
                      .Select(group => new
                      {
                          Name = group.Key,
                          ReturnType = group.Select(method => method.ReturnType).Distinct().Single()
                      });

                    var tDict = tPropReturType.Concat(tMethodReturnType)
                        .GroupBy(info => info.Name)
                        .Select(group => {
                            // TODO: what if we have properties with same name and different types?
                            return group.First();
                        })
                        .ToDictionary(info => info.Name, info => info.ReturnType);

                    _returnTypHash.Add(_hash, tDict);
                }
            }
        }

        IDictionary<string, Type> IDynamicKnowLike.KnownPropertySpec
        {
            set { KnownPropertySpec = value; }
        }

        /// <summary>
        /// Gets or sets the known fake interface (string method name to return type mapping).
        /// </summary>
        /// <value>The known fake interface.</value>
        protected virtual IDictionary<string, Type> KnownPropertySpec
        {
            get { return PropertySpec; }
            set { PropertySpec = value; }
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var tHash = HashForThisType();
            return tHash == null
                ? new String[] { }
                : tHash.Select(it => it.Key);
        }

        private IDictionary<string, Type> HashForThisType()
        {

            if (PropertySpec != null)
                return PropertySpec;
            IDictionary<string, Type> tOut;
            if (_hash == null || !_returnTypHash.TryGetValue(_hash, out tOut))
                return null;

            return tOut;
        }

        /// <summary>
        /// Tries to get the type for the property name from the interface.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnType">The return Type.</param>
        /// <returns></returns>
        public virtual bool TryTypeForName(string name, out Type returnType)
        {
            var tHash = HashForThisType();
            if (tHash == null || !tHash.TryGetValue(name, out returnType))
            {
                returnType = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Allows ActLike to be called via dyanmic invocation
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        public virtual TInterface ActLike<TInterface>(params Type[] otherInterfaces) where TInterface : class
        {
            return Impromptu.ActLike<TInterface>(this, otherInterfaces);
        }

#if SILVERLIGHT5

        /// <summary>
        /// Gets the custom Type.
        /// </summary>
        /// <returns></returns>
        public Type GetCustomType()
        {
            return this.GetDynamicCustomType();
        }
#endif
    }
}
