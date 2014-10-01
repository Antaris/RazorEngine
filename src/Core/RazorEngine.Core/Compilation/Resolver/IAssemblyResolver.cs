using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation.Resolver
{
    public interface IAssemblyReferenceResolver
    {
        IEnumerable<string> GetReferences(TypeContext context);
    }
}
