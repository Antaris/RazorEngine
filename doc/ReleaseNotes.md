### 3.7.2 / 4.2.2-beta1

 * Validate and safe-copy the configuration. Changing the configuration after creating a RazorEngineService instance is not (and was never) supported.

### 3.7.1 / 4.2.1-beta1

 * Validate and safe-copy the configuration. Changing the configuration after creating a RazorEngineService instance is not (and was never) supported.

### 3.7.1-beta1 / 4.2.0-beta3

 * Build version 4 against Mircrosoft.AspNet.Razor 4.

### 3.7.0 / 4.2.0-beta2

 * add InvalidatingCachingProvider, ResolvePathTemplateManager and WatchingResolvePathTemplateManager, fixes https://github.com/Antaris/RazorEngine/issues/250
 * Switched to Apache 2: https://github.com/Antaris/RazorEngine/issues/190
 * Missing feature (compared to 3.6): Configuration fallback to XML.

### 3.6.6 / 4.1.6-beta1
* Another attempt to fix https://github.com/Antaris/RazorEngine/issues/267

### 3.6.5 / 4.1.5-beta1
* Fix https://github.com/Antaris/RazorEngine/issues/267

### 3.6.4 / 4.1.4-beta1
* Use /nostdlib when we find a mscorlib (improves mono support)
* Added `DisableTempFileLocking` to load assemblies in memory (to prevent temp file locking), 
  this is only recommended in a very limited amount of scenarios.
  Please be aware of the consequences before using it.
  Please read https://github.com/Antaris/RazorEngine/issues/244 for details.
  Note that this can introduce memory leaks to your application.

### 3.6.3 / 4.1.3-beta2
* Cleanup temporary files when RazorEngine is not used in the default AppDomain
* Write to stderr when cleanup is not successful.
* Add an API to change the temporary directory (by subclassing CompilerService)
* Fix https://github.com/Antaris/RazorEngine/issues/253

### 3.6.3-beta2
* Fix some race conditions in cleanup.
* Fix https://github.com/Antaris/RazorEngine/issues/253

### 3.6.3-beta1 / 4.1.3-beta1
* Cleanup temporary files when RazorEngine is not used in the default AppDomain
* Add an API to change the temporary directory (by subclassing CompilerService)

### 3.6.2 / 4.1.2-beta1
* Add support for escaping the dynamic wrapper in certain situations
  - Implicit or explicit casts
  - By using `RazorEngine.Compilation.RazorDynamicObject.Unwrap(Model.ToUnwrap)`
* Wrapper now supports binary and unary operations (thanks devteamexpress): https://github.com/Antaris/RazorEngine/issues/248

### 3.6.1 / 4.1.1-beta1
* Add support for nested model types as well
* Fix a bug with non-generic nested template base classes.

### 3.6.0 / 4.1.0-beta1
* ICodeInspector API is now obsolete (it has been removed in 4.0.0).
* Nested classes can now be used as template-base-class.
* ViewBag data can now be used in overwritten SetModel calls
  Viewbag is set like the model on template creation (not in the ExecuteContext)
  This leads to some minor breaking changes
  - Creating a ExecuteContext with a non-null viewbag throws a NotSupportedException (obsolete API)
  - added a DynamicViewBag parameter to IInternalTemplateService.Resolve (you should not need to use that API)
  - ITemplate.SetModel has changed to ITemplate.SetData (you are not affected if you use TemplateBase or TemplateBase<T> as base class, which is recommended)
  
These changes are strictly speaking breaking, but they shouldn't practically affect anyone.
Watch out for new Obsolete warnings and fix them.

### 3.5.3 / 4.0.3-beta1
* Fixed a SecurityException when the template is broken.
* RazorEngine is now compatible with ImpromptuInterface (thanks kipwilliams).

### 3.5.2 / 4.0.2-beta1
* If you use assemblies in your template which are not referenced in your hosting code 
  RazorEngine will load them for you automatically if they are required.
  See https://github.com/Antaris/RazorEngine/issues/218 for details.

### 3.5.1 / 4.0.1-beta1
* Fixed https://github.com/Antaris/RazorEngine/issues/217

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