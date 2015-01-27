using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Compilation.ImpromptuInterface
{


    /// <summary>
    /// String or InvokeMemberName
    /// </summary>
    [Serializable]
    public abstract class String_OR_InvokeMemberName
    {
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="RazorEngine.Compilation.ImpromptuInterface.String_OR_InvokeMemberName"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator String_OR_InvokeMemberName(string name)
        {
            return new InvokeMemberName(name, null);
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; protected set; }
        /// <summary>
        /// Gets the generic args.
        /// </summary>
        /// <value>The generic args.</value>
        public Type[] GenericArgs { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this member is special name.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is special name; otherwise, <c>false</c>.
        /// </value>
        public bool IsSpecialName { get; protected set; }
    }

    /// <summary>
    /// Name of Member with associated Generic parameterss
    /// </summary>
    [Serializable]
    public sealed class InvokeMemberName : String_OR_InvokeMemberName
    {
        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<string, Type[], InvokeMemberName> Create =
            new Func<string, Type[], InvokeMemberName>((n, a) => new InvokeMemberName(n, a));

        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<string, InvokeMemberName> CreateSpecialName =
          new Func<string, InvokeMemberName>(n => new InvokeMemberName(n, true));

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="RazorEngine.Compilation.ImpromptuInterface.InvokeMemberName"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator InvokeMemberName(string name)
        {
            return new InvokeMemberName(name, null);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeMemberName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="genericArgs">The generic args.</param>
        public InvokeMemberName(string name, params Type[] genericArgs)
        {
            Name = name;
            GenericArgs = genericArgs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeMemberName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isSpecialName">if set to <c>true</c> [is special name].</param>
        public InvokeMemberName(string name, bool isSpecialName)
        {
            Name = name;
            GenericArgs = new Type[] { };
            IsSpecialName = isSpecialName;
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(InvokeMemberName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualsHelper(other);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        private bool EqualsHelper(InvokeMemberName other)
        {

            var tGenArgs = GenericArgs;
            var tOtherGenArgs = other.GenericArgs;


            return Equals(other.Name, Name)
                && !(other.IsSpecialName ^ IsSpecialName)
                && !(tOtherGenArgs == null ^ tGenArgs == null)
                && (tGenArgs == null ||
                //Exclusive Or makes sure this doesn't happen
                // ReSharper disable AssignNullToNotNullAttribute
                tGenArgs.SequenceEqual(tOtherGenArgs));
            // ReSharper restore AssignNullToNotNullAttribute
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
            if (!(obj is InvokeMemberName)) return false;
            return EqualsHelper((InvokeMemberName)obj);
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
                return (GenericArgs != null ? GenericArgs.GetHashCode() * 397 : 0) ^ (Name.GetHashCode());
            }
        }
    }
}
