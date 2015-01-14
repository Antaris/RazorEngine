
### 3.5.0 / 4.0.0-beta2
* documentation website based on FSharp.Formatting (https://Antaris.github.io/RazorEngine), can be build locally
* travis and appveyor build systems to automatically build on mono and windows.
* build environment to build and release the project, the documentation and the nuget package.
* readded support for net40 (which will use the Razor with version 2.0.30506.0)
* New API surface (various classes are Obsolete now).
* Improved Isolation (AppDomain Sandboxing) support.
* Caching API.
* Fixed most open bugs.
* Razor 4 support (in 4.0.0)
* Roslyn Support (via RazorEngine.Roslyn, not supported on net40)
* Please report breaking changes in 3.5.0 (from 3.4.x) and open an issue!

### 3.5.0-beta3 / 4.0.0-beta2
* Fix a missing [SecurityCritical] attribute on TemplateParsingException.GetObjectData

### 3.5.0-beta2
* Relax nuget Razor dependency

### New in 3.5.0-beta1 / 4.0.0-beta1 (To be released) 
* documentation website based on FSharp.Formatting (https://Antaris.github.io/RazorEngine), can be build locally
* travis and appveyor build systems to automatically build on mono and windows.
* build environment to build and release the project, the documentation and the nuget package.
* readded support for net40 (which will use the Razor with version 2.0.30506.0)
* New API surface (various classes are Obsolete now).
* Improved Isolation (AppDomain Sandboxing) support.
* Caching API.
* Fixed most open bugs.
* Razor 4 support (in 4.0.0)
* Roslyn Support (via RazorEngine.Roslyn, not supported on net40)
* Please report breaking changes in 3.5.0-beta1 (from 3.4.x) and open an issue!