# RazorEngine implementation documentation 

## Building

Open the ``.sln`` file or run ``./build.cmd``.

## General overview:

This project aims to be a very flexible, extendable and good performing templating engine based on the Razor parser (Microsoft.AspNet.Razor -> System.Web.Razor.dll).
Everybody familiar with Razor can start building templates very quickly.

### Issues / Features / TODOs

Issues and TODOs are tracked on github:
See: https://github.com/Anatris/RazorEngine/issues or https://github.com/matthid/RazorEngine/issues

### Versioning: 

Not clear.
 
### High level documentation ordered by project.

- RazorEngine.Core

	The Core of the RazorEngine framework, basically all you need to get started.

- RazorEngine.Mvc

	A template base-class implementation which uses MVC classes to get the @Html and @Url features.

- RazorEngine.Hosts.Console

	A simple console application showing the beauty of RazorEngine.


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

	Project documentation files. This folder contains both development and user documentation (currently only development documentation because the project is at an very early phase).

- /src/

	The Solution directory for all projects

	- /src/source/

		The root for all projects (not including unit test projects)

	- /src/test/

		The root for all unit test projects

- /test/

	The unit test assemblies will be build into this folder. This folder can safly be deleted without affecting the build.

- /tmp/

	This folder is ignored by git.

- /build.cmd, /build.sh, /build.fsx

	Files to directly start a build including unit tests via console (windows & linux).

-  /packages.config

	Nuget packages required by the build process.



Each project should have has a corresponding project with the name "Test.${ProjectName}" in the test folder.
This test project provides unit tests for the project "${ProjectName}". 
Often unit tests are written agains interfaces and used by multiple implementations of the interface.
Sometimes these unit tests can be used even by custom code (inheriting from the corresponding test-class and overriding an Create* method). 

