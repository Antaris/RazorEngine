using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation.ReferenceResolver
{
    /// <summary>
    /// Resolves the assemblies by using all currently loaded assemblies. See <see cref="IReferenceResolver"/>
    /// </summary>
    public class UseCurrentAssembliesReferenceResolver : IReferenceResolver
    {
        [SecuritySafeCritical]
        private static IEnumerable<CompilerReference> GetReferencesPrivate(IEnumerable<CompilerReference> includeAssemblies = null)
        {
            return CompilerServicesUtility
                   .GetLoadedAssemblies()
                   .Concat(new[] {
                       typeof(System.Collections.Immutable.ImmutableArray).Assembly,
                       typeof(System.Reflection.Metadata.AssemblyDefinition).Assembly
                   })
                   .Where(a => IsValidAssembly(a))
                   .GroupBy(a => GetName(a)).Select(grp => grp.First(y => GetName(y).Version == grp.Max(x => GetName(x).Version))) // only select distinct assemblies based on FullName to avoid loading duplicate assemblies
                   .Select(a => CompilerReference.From(a))
                   .Concat(includeAssemblies ?? Enumerable.Empty<CompilerReference>());
        }

        [SecuritySafeCritical]
        private static AssemblyName GetName(System.Reflection.Assembly a)
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            return a.GetName();
        }

        [SecuritySafeCritical]
        private static bool IsValidAssembly(System.Reflection.Assembly a)
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            return !a.IsDynamic && File.Exists(a.Location) && !a.Location.Contains(CompilerServiceBase.DynamicTemplateNamespace);
        }

        /// <summary>
        /// See <see cref="IReferenceResolver.GetReferences"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="includeAssemblies"></param>
        /// <returns></returns>
        public IEnumerable<CompilerReference> GetReferences(TypeContext context = null, IEnumerable<CompilerReference> includeAssemblies = null)
        {
            return GetReferencesPrivate(includeAssemblies);
        }
    }
}
