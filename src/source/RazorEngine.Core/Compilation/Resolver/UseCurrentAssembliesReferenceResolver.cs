using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RazorEngine.Compilation.Resolver
{
    public class UseCurrentAssembliesReferenceResolver : IAssemblyReferenceResolver
    {
        public IEnumerable<string> GetReferences(TypeContext context = null, IEnumerable<string> includeAssemblies = null)
        {
            return CompilerServicesUtility
                   .GetLoadedAssemblies()
                   .Where(a => !a.IsDynamic && File.Exists(a.Location))
                   .GroupBy(a => a.GetName().Name).Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
                   .Select(a => a.Location)
                   .Concat(includeAssemblies ?? Enumerable.Empty<string>());
        }
    }
}
