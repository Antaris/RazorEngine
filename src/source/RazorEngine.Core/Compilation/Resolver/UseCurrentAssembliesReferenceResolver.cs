using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RazorEngine.Compilation.Resolver
{
    /// <summary>
    /// Resolves the assemblies by using all currently loaded assemblies. See <see cref="IAssemblyReferenceResolver"/>
    /// </summary>
    public class UseCurrentAssembliesReferenceResolver : IAssemblyReferenceResolver
    {
        /// <summary>
        /// See <see cref="IAssemblyReferenceResolver.GetReferences"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="includeAssemblies"></param>
        /// <returns></returns>
        public IEnumerable<string> GetReferences(TypeContext context = null, IEnumerable<string> includeAssemblies = null)
        {
            return CompilerServicesUtility
                   .GetLoadedAssemblies()
                   .Where(a => !a.IsDynamic && File.Exists(a.Location) && !a.Location.Contains(CompilerServiceBase.DynamicTemplateNamespace))
                   .GroupBy(a => a.GetName().Name).Select(grp => grp.First(y => y.GetName().Version == grp.Max(x => x.GetName().Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
                   .Select(a => a.Location)
                   .Concat(includeAssemblies ?? Enumerable.Empty<string>());
        }
    }
}
