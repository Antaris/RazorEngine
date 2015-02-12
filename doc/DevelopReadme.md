# RazorEngine implementation documentation 

## Building

Open the ``.sln`` file or run ``./build``.

> NOTE: It is possible that you can only build the ``.sln`` file AFTER doing an initial ``./build`` (because nuget dependencies have to be resolved).

## General overview:

This project aims to be a very flexible, extendable and good performing templating engine based on the Razor parser (Microsoft.AspNet.Razor -> System.Web.Razor.dll).
Everybody familiar with Razor can start building templates very quickly.

### Issues / Features / TODOs

New features are accepted via github pull requests (so just fork away right now!): https://github.com/Anatris/RazorEngine

Issues and TODOs are tracked on github, see: https://github.com/Anatris/RazorEngine/issues

Discussions/Forums can be found here: https://groups.google.com/forum/#!forum/razorengine

### Versioning: 

http://semver.org/ like versioning.

#### X.Y.Z (Major)

X is the major Razor-Parser version that is used. Different X numbers are not guaranteed to be runtime-compatible.

Y is the minor version. Y version changes add features and fix bugs but "should" be backwards compatible.
Breaks can happen if you use the API in an unexpected way.

Z version changes are bug-fixes and are highly backwards compatible.

#### General considerations.

* The 4.x build is compatible with Razor4 while the 3.x build is compatible with Razor3 (OR Razor2 for the .net40 build).
* 3.y.z should be compile compatible with 4.(y-5).z (has the same API/feature but uses another version of the Razor parser)
* 3.y.z (net40, net45) and 4.(y-5).z (net45) are generally build from the same source code state.
* Incrementing the X version should be compile-compatible if you do not use any obsoleted APIs and use the corresponding Y version.
* Incrementing the Y version should be backwards compatible, if you do not use the API in unexpected ways.
* Incrementing the Z version is fully backwards compatible.

### High level documentation ordered by project.

- `RazorEngine.Core`: The Core of the RazorEngine framework, basically all you need to get started.

- `RazorEngine.Core.Roslyn`: Support for Roslyn.

- `RazorEngine.Mvc`: A template base-class implementation which uses MVC classes to get the @Html and @Url features.

- `RazorEngine.Hosts.Console`: A simple console application showing the beauty of RazorEngine.


### The Project structure:

- /.nuget/

	Nuget dependencies will be downloaded into this folder. 
	This folder can safely be deleted without affecting the build.

- /build/

	The project assemblies will be build into this folder. This folder can safely be deleted without affecting the build.

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

	The unit test assemblies will be build into this folder. This folder can safely be deleted without affecting the build.

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

## Visual Studio / Monodevelop

As mentioned above you need to `build` at least once before you can open the 
solution file (`src/RazorEngine.sln`) with Visual Studio / Monodevelop.

The default is that Visual Studio is configured for the latest build (`razor4`).
If you want to build another target with Visual Studio / Monodevelop do the following:

 - Close the solution
 - Open `src/buildConfig.targets` and change the `CustomBuildName` entry (near the top) to `razor4`, `net45` or `net40`.
   The line should then look like this:
   
   ```markup
   <CustomBuildName Condition=" '$(CustomBuildName)' == '' ">net45</CustomBuildName> 
   ```

 - Save the `src/buildConfig.targets` file and re-open the solution.

## Bootstrapping FSharp.Formatting (normally not required)

For generation of the documentation website we use ourself (because FSharp.Formatting uses RazorEngine) 
and in fact we use the latest build (done immediately before generating the docs) so this is a integration test as well!
We normally use the official build (nuget package) of FSharp.Formatting, but if we introduce breaking changes
we need to update FSharp.Formatting to make the documentation generation work again.

This is the reason why we have a bootstrap documentation for FSharp.Formatting:

 - Clone RazorEngine (C:/Projects/RazorEngine)
 - Clone FSharp.Formatting (C:/Projects/FSharp.Formatting)
 - Build RazorEngine (./build All, it doesn't matter if generating the documentation fails!)
 - do `build NuGet_single` to generate the nuget packages
 - Edit `C:/Projects/FSharp.Formatting/paket.lock` so that it loads the RazorEngine nuget package from disk:
 
   ```markup
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
     remote: C:\Projects\RazorEngine\release\nuget
     specs:
       RazorEngine (3.5.0)
         Microsoft.AspNet.Razor (>= 3.2.2.0) - net45
         Microsoft.AspNet.Razor (2.0.30506.0) - net40
   ```

 - Delete `C:/Projects/FSharp.Formatting/packages/RazorEngine` if it exists.
 - Fix all compile errors (-> adapt FSharp.Formatting to our latest build) and build FSharp.Formatting.
 - Your FSharp.Formatting build can be found in C:/Projects/FSharp.Formatting/bin
 - Please send your changes back to FSharp.Formatting.
 - You can now bundle this build with RazorEngine (link the pull request to FSharp.Formatting in your pull request).

