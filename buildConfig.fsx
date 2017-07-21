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
                    Dependencies = [ "Microsoft.AspNet.Razor", "3.0.0" ] } 
                  { FrameworkVersion = "net451";
                    Dependencies =
                      [ "Microsoft.AspNetCore.Razor", "1.1.2"
                        "System.Collections.Immutable", "1.2.0"
                        "System.Reflection.Metadata", "1.4.2"
                      ] } ] })
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
     )
    DocRazorReferences = None
    BuildTargets =
     [ { BuildParams.WithSolution with
          // The razor4 (net451) build
          PlatformName = "Razor4"
          SimpleBuildName = "net451"
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

