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
using System.Linq;
using System.Reflection;
using System.Text;

namespace RazorEngine.Compilation.ImpromptuInterface.Build
{
    /// <summary>
    /// Type that Encompasses Hashing a group of Types in various ways
    /// </summary>
    public class TypeHash
    {
        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(TypeHash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (InformalInterface != null || other.InformalInterface != null)
            {
                if (InformalInterface == null || other.InformalInterface == null)
                    return false;

                if (Types.Length != other.Types.Length)
                    return false;

                var tTypes = Types.SequenceEqual(other.Types);

                if (!tTypes)
                    return false;

                return InformalInterface.SequenceEqual(other.InformalInterface);
            }


            return Types.SequenceEqual(other.Types);
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
            if (obj.GetType() != typeof(TypeHash)) return false;
            return Equals((TypeHash)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {

                if (Types.Length > 16)
                {
                    return Types.Length.GetHashCode();
                }


                var tReturn = Types.Aggregate(1, (current, type) => (current * 397) ^ type.GetHashCode());

                if (InformalInterface != null)
                {
                    tReturn = InformalInterface.Aggregate(tReturn, (current, type) => (current * 397) ^ type.GetHashCode());
                }
                return tReturn;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(TypeHash left, TypeHash right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(TypeHash left, TypeHash right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Types to be hashed
        /// </summary>
        public readonly MemberInfo[] Types;

        /// <summary>
        /// The Informal Interface to be hashed
        /// </summary>
        public readonly IDictionary<string, Type> InformalInterface;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHash"/> class.
        /// </summary>
        /// <param name="moreTypes">The more types.</param>
        [Obsolete("Use TypeHash.Create instead.")]
        public TypeHash(IEnumerable<Type> moreTypes)
            : this(false, moreTypes.ToArray())
        {

        }

        private TypeHash()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHash"/> class.
        /// For use when you have must distinguish one type; and the rest aren't strict
        /// </summary>
        /// <param name="type1">The type1.</param>
        /// <param name="moreTypes">The more types.</param>
        [Obsolete("Use TypeHash.Create instead.")]
        public TypeHash(Type type1, params Type[] moreTypes)
            : this()
        {
            Types = new[] { type1 }.Concat(moreTypes.OrderBy(it => it.Name)).ToArray();
            InformalInterface = null;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHash"/> class.
        /// </summary>
        /// <param name="type1">The type1.</param>
        /// <param name="informalInterface">The informal interface.</param>
        [Obsolete("Use TypeHash.Create instead.")]
        public TypeHash(Type type1, IDictionary<string, Type> informalInterface)
            : this()
        {
            Types = new[] { type1 };
            InformalInterface = informalInterface;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeHash"/> class.
        /// </summary>
        /// <param name="strictOrder">if set to <c>true</c> [strict order].</param>
        /// <param name="moreTypes">types.</param>
        [Obsolete("Use TypeHash.Create instead.")]
        public TypeHash(bool strictOrder, params MemberInfo[] moreTypes)
            : this()
        {
            Types = strictOrder
                ? moreTypes
                : moreTypes.OrderBy(it => it.Name).ToArray();
        }

        /// <summary>
        /// Creates the TypeHash
        /// </summary>
        /// <param name="moreTypes">The more types.</param>
        /// <returns></returns>
        public static TypeHash Create(IEnumerable<Type> moreTypes)
        {
#pragma warning disable 612,618
            return new TypeHash(moreTypes);
#pragma warning restore 612,618
        }
        /// <summary>
        /// Creates the TypeHash
        /// </summary>
        /// <param name="type1">The type1.</param>
        /// <param name="moreTypes">The more types.</param>
        /// <returns></returns>
        public static TypeHash Create(Type type1, params Type[] moreTypes)
        {
#pragma warning disable 612,618
            return new TypeHash(type1, moreTypes);
#pragma warning restore 612,618
        }
        /// <summary>
        /// Creates the TypeHash
        /// </summary>
        /// <param name="type1">The type1.</param>
        /// <param name="informalInterface">The informal interface.</param>
        /// <returns></returns>
        public static TypeHash Create(Type type1, IDictionary<string, Type> informalInterface)
        {
#pragma warning disable 612,618
            return new TypeHash(type1, informalInterface);
#pragma warning restore 612,618
        }
        /// <summary>
        /// Creates the TypeHash
        /// </summary>
        /// <param name="strictOrder">if set to <c>true</c> [strict order].</param>
        /// <param name="moreTypes">The more types.</param>
        /// <returns></returns>
        public static TypeHash Create(bool strictOrder, params MemberInfo[] moreTypes)
        {
#pragma warning disable 612,618
            return new TypeHash(strictOrder, moreTypes);
#pragma warning restore 612,618
        }
    }
}
