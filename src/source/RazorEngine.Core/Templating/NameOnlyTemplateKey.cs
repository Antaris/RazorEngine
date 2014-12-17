using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    public class NameOnlyTemplateKey : BaseTemplateKey
    {

        public NameOnlyTemplateKey(string name, ResolveType resolveType, ITemplateKey context)
            : base(name, resolveType, context) { }

        public override string GetUniqueKeyString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as NameOnlyTemplateKey;
            if (object.ReferenceEquals(null, other))
            {
                return false;
            }
            return other.Name == Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
