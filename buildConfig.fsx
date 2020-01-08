// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the configuration of the Yaaf.AdvancedBuilding build script.

    The first step is handled in build.sh and build.cmd by restoring either paket dependencies or bootstrapping a NuGet.exe and
    executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE).

    The secound step is executing build.fsx which loads this file (for configuration), builds the solution and executes all unit tests.
*)

#if FAKE
#else
// Support when file is opened in Visual Studio
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#endif

open BuildConfigDef
open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"

// !!!!!!!!!!!!!!!!!!!
// UPDATE RELEASE NOTES AS WELL! (set 'nugetkey' environment variable to push directly.)
// !!!!!!!!!!!!!!!!!!!
let version_razor4 = "4.5.1-alpha003"

let unitTestFinder (testDir, (buildParams:BuildParams)) =
    let items = !! (testDir + "/Test.*.dll")
    (if not isMono then items else
     items
     -- (testDir + "/Test.*.Roslyn.dll"))
    :> _ seq

let buildConfig =
 // Read release notes document
 let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
 { BuildConfiguration.Defaults with
    ProjectName = "RazorEngine"
    CopyrightNotice = "RazorEngine Copyright © RazorEngine Project 2011-2017"
    ProjectSummary = "Simple templating using Razor syntax."
    ProjectDescription = "RazorEngine - A Templating Engine based on the Razor parser."
    ProjectAuthors = ["Matthew Abbott"; "Ben Dornis"; "Matthias Dittrich"]
    NugetTags =  "C# razor template engine programming"
    PageAuthor = "Matthias Dittrich"
    GithubUser = "Antaris"
    Version = release.NugetVersion
    NugetPackages =
      [ "RazorEngine.nuspec", (fun config p ->
          { p with
              Version = config.Version
              ReleaseNotes = toLines release.Notes
              DependenciesByFramework =
                [ { FrameworkVersion = "net40";
                    Dependencies = [ "Microsoft.AspNet.Razor", "2.0.30506.0" |> RequireExactly ] }
                  { FrameworkVersion = "net45";
                    Dependencies = [ "Microsoft.AspNet.Razor", "3.0.0" ] } ] })
        "RazorEngine-razor4.nuspec", (fun config p ->
          { p with
              Version = version_razor4
              ReleaseNotes = toLines release.Notes
              DependenciesByFramework =
                [ { FrameworkVersion = "net451";
                    Dependencies =
                      [ "Microsoft.AspNetCore.Razor", "1.1.2" |> RequireExactly
                        "Microsoft.CodeAnalysis", "1.3.2"
                        "System.Collections.Immutable", "1.3.1"
                        "System.Reflection.Metadata", "1.4.2"
                      ] }
                ]
          })
      ]
    UseNuget = false
    DisableMSTest = true
    DisableNUnit = true
    GeneratedFileList =
      [ "RazorEngine.dll"; "RazorEngine.xml" ]
    SetAssemblyFileVersions = (fun config ->
      let info =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version config.Version
          Attribute.FileVersion config.Version
          Attribute.InformationalVersion config.Version ]
      CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info
      let info_razor4 =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version version_razor4
          Attribute.FileVersion version_razor4
          Attribute.InformationalVersion version_razor4 ]
      CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo-Razor4.cs" info_razor4
     )
    DocRazorReferences = None
    BuildTargets =
     [ { BuildParams.WithSolution with
          // The net40 (razor2) build
          PlatformName = "Net40"
          SimpleBuildName = "net40"
          FindUnitTestDlls = unitTestFinder }
       { BuildParams.WithSolution with
          // The razor4 (net45) build
          PlatformName = "Razor4"
          SimpleBuildName = "razor4"
          FindUnitTestDlls = unitTestFinder }
       { BuildParams.WithSolution with
          // The net45 (razor3) build
          PlatformName = "Net45"
          SimpleBuildName = "net45"
          FindUnitTestDlls = unitTestFinder } ]
    SetupNUnit = (fun p ->
      { p with
          //NUnitParams.WorkingDir = working
          ExcludeCategory = if isMono then "VBNET" else "" })
    SetupNUnit3 = (fun p ->
      { p with
          //NUnit3Params.WorkingDir = working
          Where = if isMono then "cat!=VBNET" else "" })
  }

