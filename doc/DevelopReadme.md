# RazorEngine implementation documentation 

## Building

Open the ``.sln`` file or run ``./build``.

> NOTE: It is possible that you can only build the ``.sln`` file AFTER doing an initial ``./build`` (because nuget dependencies have to be resolved).

## General overview:

This project aims to be a very flexible, extendable and good performing templating engine based on the Razor parser (Microsoft.AspNet.Razor -> System.Web.Razor.dll).
Everybody familiar with Razor can start building templates very quickly.

### Issues / Features / TODOs

New features are accepted via github pull requests (so just fork away right now!): https://github.com/Anatris/RazorEngine.

Issues and TODOs are tracked on github, see: https://github.com/Anatris/RazorEngine/issues.

Discussions/Forums can be found here: https://groups.google.com/forum/#!forum/razorengine. 

### Versioning: 

http://semver.org/

### High level documentation ordered by project.

- `RazorEngine.Core`: The Core of the RazorEngine framework, basically all you need to get started.

- `RazorEngine.Core.Roslyn`: Support for Roslyn.

- `RazorEngine.Mvc`: A template base-class implementation which uses MVC classes to get the @Html and @Url features.

- `RazorEngine.Hosts.Console`: A simple console application showing the beauty of RazorEngine.


### The Project structure:

- /.nuget/

	Nuget dependencies will be downloaded into this folder. 
	This folder can safly be deleted without affecting the build.

- /build/

	The project assemblies will be build into this folder. This folder can safly be deleted without affecting the build.

- /lib/

	library dependencies. Most dependencies are automatically managed by nuget and not in this folder. 
	Only some internal dependencies and packages not in nuget. The git repository should always be "complete".

- /doc/

	Project documentation files. This folder contains both development and user documentation.

- /src/

	The Solution directory for all projects

	- /src/source/

		The root for all projects (not including unit test projects)

	- /src/test/

		The root for all unit test projects.

	- /src/nugetDependencies/

		A bit of a hack to get the dependencies of the .net40 profile downloaded properly.
		
- /test/

	The unit test assemblies will be build into this folder. This folder can safly be deleted without affecting the build.

- /tmp/

	This folder is ignored by git.

- /build.cmd, /build.sh, /build.fsx

	Files to directly start a build including unit tests via console (windows & linux).

-  /packages.config

	Nuget packages required by the build process.


Each project should have has a corresponding project with the name `Test.${ProjectName}` in the test folder.
This test project provides unit tests for the project `${ProjectName}`.

## Advanced Building

The build is done in different steps and you can execute the build until a given step or a single step:

First `build.sh` and `build.cmd` restore build dependencies and `nuget.exe`, then build.fsx is invoked:

 - `Clean`: cleans the directories (previous builds)
 - `RestorePackages`: restores nuget packages
 - `SetVersions_Razor4`: set 4.0.0
 - `BuildApp_Razor4`: build with razor4 (net45)
 - `BuildTest_Razor4`: build the tests with razor4 (net45)
 - `Test_Razor4`: run the tests with razor4 (net45)
 - `SetVersions`: set 3.5.0
 - `BuildApp_40`: build with razor2 (net40)
 - `BuildTest_40`: build the tests with razor2 (net40)
 - `Test_40`: run the tests with razor2 (net40)
 - `BuildApp_45`: build with razor3 (net45)
 - `BuildTest_45`: build the tests with razor3 (net45)
 - `Test_45`: run the tests with razor3 (net45)
 - `CopyToRelease`: copy the generated .dlls to release/lib
 - `LocalDoc`: create the local documentation you can view that locally
 - `All`: this does nothing itself but is used as a marker (executed by default when no parameter is given to ./build)
 - `VersionBump`: commits all current changes (when you change the version before you start the build you will have some files changed)
 - `NuGet`: generates the nuget packages
 - `GithubDoc`: generates the documentation for github
 - `ReleaseGithubDoc`: pushes the documentation to github
 - `Release`: a marker like "All"

You can execute all steps until a given point with `./build #Step#` (replace #Step# with `Test_Razor4` to execute `Clean`, `RestorePackages`, `SetVersions_Razor4`, ..., `Test_Razor4`)

You can execute a single step with `build #Step#_single`: For example to build the nuget packages you can just invoke `./build NuGet_single` 

> Of course you need to have the appropriate dlls in place (otherwise the Nuget package creation will fail); ie have build RazorEngine before.


There is another (hidden) step `CleanAll` which will clean everything up (even build dependencies and the downloaded Nuget.exe), 
this step is only needed when build dependencies change. `git clean -d -x -f` is also a good way to do that

## Documentation generation

For the docs we use a custom build of FSharp.Formatting (because of improved C# support and to make use of the latest RazorEngine features!)
and FSharp.Compiler.Service (see lib/ folder).
You normally do not need to build those yourself. For the sake of completness all steps are included here.
> Note: The aim is to switch to the official versions once everything works with them 
> (pull requests are open... for example FSharp.Compiler.Service: #229, FSharp.Formatting: #208 and others)

### FSharp.Formatting

So for generation of the docs we use ourself (because FSharp.Formatting uses RazorEngine) and in fact we use the latest build 
(done immediatly before generating the docs) so this is a integration test as well!

The custom build can be found on https://github.com/matthid/FSharp.Formatting/tree/razor_engine_bundled (the `razor_engine_bundled` branch).
If you want to build that (because you make a breaking change, 
please only make breaking changes when the change was never included in a release) you need to do the following:

 - Clone RazorEngine (C:/Projects/RazorEngine)
 - Clone the `razor_engine_bundled` branch of FSharp.Formatting (C:/Projects/FSharp.Formatting)
 - Build RazorEngine (./build All, it doesn't matter if generating the documentation fails!)
 - do `build NuGet_single` to generate the nuget packages
 - Edit `C:/Projects/FSharp.Formatting/paket.lock` so that it loads the RazorEngine nuget package from disk:

		 NUGET
		  remote: http://nuget.org/api/v2
		  specs:
			CommandLineParser (1.9.71)
			FAKE (3.12.2)
			FSharp.Compiler.Service (0.0.67)
			Microsoft.AspNet.Razor (2.0.30506.0)
			NuGet.CommandLine (2.8.3)
			NUnit (2.6.4)
			NUnit.Runners (2.6.4)
		  remote: C:\\Projects\\RazorEngine\\release\\nuget
		  specs:
			RazorEngine (3.5.0-beta1)
			  Microsoft.AspNet.Razor (>= 3.2.2.0) - net45
			  Microsoft.AspNet.Razor (2.0.30506.0) - net40

 - Delete `C:/Projects/FSharp.Formatting/packages/RazorEngine` if it exists.
 - Build FSharp.Formatting.
 - Your FSharp.Formatting build can be found in C:/Projects/FSharp.Formatting/bin

### FSharp.Compiler.Service

FSharp.Compiler.Service doesn't depend on anything so building the custom version is just a matter of cloning and building: 
https://github.com/matthid/FSharp.Compiler.Service (`master` branch).
The changes are not breaking so using the official version will work as well, however the docs are incomplete in that case!