using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A simple <see cref="ITemplateKey"/> implementation inheriting from <see cref="BaseTemplateKey"/>.
    /// This implementation assumes that the template-names are unique and returns the name as unique key.
    /// (So this implementation is used by <see cref="DelegateTemplateManager"/> and <see cref="RazorEngine.Configuration.Xml.WrapperTemplateManager"/>.
    /// </summary>
    [Serializable]
    public class NameOnlyTemplateKey : BaseTemplateKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NameOnlyTemplateKey"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        public NameOnlyTemplateKey(string name, ResolveType resolveType, ITemplateKey context)
            : base(name, resolveType, context) { }

        /// <summary>
        /// Returns the name.
        /// </summary>
        /// <returns></returns>
        public override string GetUniqueKeyString()
        {
            return this.Name;
        }

        /// <summary>
        /// Checks if the names are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as NameOnlyTemplateKey;
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return other.Name == Name;
        }

        /// <summary>
        /// Returns a hashcode for the current instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
