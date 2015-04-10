# Reference Resolver

Templates are first transformed to a source code file and then dynamically compiled by invoking the compiler.
Because you can use source code within the template itself you are free to use any libraries within a template.
However the compiler needs to be able to resolve everything and the default strategy is to reference all currently loaded assemblies.
This can lead to problems when you want to use a library (in the template) which is not referenced in the 
hosting code or not loaded by the runtime (because it is unused).
It is also possible that you run into problems on Mono because mcs behaves differently.
To be able to resolve such issues you can control this behaviour and set your own `IReferenceResolver` implementation.

```csharp
config.ReferenceResolver = new MyIReferenceResolver();

class MyIReferenceResolver : IReferenceResolver {
    public string FindLoaded(IEnumerable<string> refs, string find) {
        return refs.First(r => r.EndsWith(System.IO.Path.DirectorySeparatorChar + find));
    }
    public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies) {
        // TypeContext gives you some context for the compilation (which templates, which namespaces and types)
        
        // You must make sure to include all libraries that are required!
        // Mono compiler does add more standard references than csc! 
        // If you want mono compatibility include ALL references here, including mscorlib!
        // If you include mscorlib here the compiler is called with /nostdlib.
        IEnumerable<string> loadedAssemblies = (new UseCurrentAssembliesReferenceResolver())
            .GetReferences(context, includeAssemblies)
            .Select(r => r.GetFile())
            .ToArray();

        yield return CompilerReference.From(FindLoaded(loadedAssemblies, "mscorlib.dll"));
        yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.dll"));
        yield return CompilerReference.From(FindLoaded(loadedAssemblies, "System.Core.dll"));
        yield return CompilerReference.From(typeof(MyIReferenceResolver).Assembly); // Assembly
        
        // There are several ways to load an assembly:
        //yield return CompilerReference.From("Path-to-my-custom-assembly"); // file path (string)
        //byte[] assemblyInByteArray = --- Load your assembly ---;
        //yield return CompilerReference.From(assemblyInByteArray); // byte array (roslyn only)
        //string assemblyFile = --- Get the path to the assembly ---;
        //yield return CompilerReference.From(File.OpenRead(assemblyFile)); // stream (roslyn only)
    }
}

```

By default the `UseCurrentAssembliesReferenceResolver` class is used, which will always returns all currently loaded assemblies, 
which can be resolved to a file (so this excludes dynamic assemblies).

> Instead of trying to make it work without /nostdlib it is useful to just manually return
> all assemblies (including mscorlib.dll) to stay compatible with mono.
