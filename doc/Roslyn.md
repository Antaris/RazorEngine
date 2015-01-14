
# Roslyn compiler support

Starting with 3.5.0 RazorEngine supports the Roslyn compilers (via the Microsoft.CodeAnalysis nuget package).
The first thing you want to do is install the additional package:

	Install-Package RazorEngine.Roslyn

> If you use the 4.x series (RazorEngine) you need the 4.x series of RazorEngine.Roslyn.

To activate roslyn all you need to do is set the `CompilerServiceFactory` property in the configuration:

```csharp
config.CompilerServiceFactory = new RazorEngine.Roslyn.RoslynCompilerServiceFactory();
```

## Known Limitation/Bugs

- Debugging symbols do not work currently (If you know how Roslyn works please send a pull request!).
- Debug symbols cannot be created on mono/unix:

         error: (0, 0) Unexpected error writing debug information -- 'The requested feature is not implemented.'

- No support for the net40 build as the roslyn nuget package doesn't support net40!
- Only C# support (VB.net support is kind of gone as Razor4 doesn't support VB.net).
- If you find more please open a issue!
- Running Roslyn on mono could lead to sigsegv crashes (mono bug): https://travis-ci.org/Antaris/RazorEngine/builds/45375847!
