using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation.Resolver
{
    /// <summary>
    /// Represents an instance which resolves the assembly references of a template.
    /// </summary>
    public interface IAssemblyReferenceResolver
    {
        /// <summary>
        /// Given the context and the 'includeAssemblies'
        /// returns the list of references which should be included.
        /// </summary>
        /// <param name="context">the context</param>
        /// <param name="includeAssemblies">assemblies which should be included, because requested by the compiler service</param>
        /// <returns></returns>
        IEnumerable<string> GetReferences(TypeContext context, IEnumerable<string> includeAssemblies);
    }
}
