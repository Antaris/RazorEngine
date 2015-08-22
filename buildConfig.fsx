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

// make the FSF load script happy
[ "build/net45/RazorEngine.dll"; "packages/Microsoft.AspNet.Razor/lib/net45/System.Web.Razor.dll" ]
|> Seq.iter (fun source ->
  let dest = sprintf "packages/FSharp.Formatting/lib/net40/%s" (Path.GetFileName source)
  try
    if File.Exists dest then File.Delete dest
    File.Copy (source, dest)
  with e ->
    trace (sprintf "Couldn't copy %s to %s, because: %O" source dest e)
)

let projectName_roslyn = "RazorEngine.Roslyn"
let projectSummary_roslyn = "Roslyn extensions for RazorEngine."
let projectDescription_roslyn = "RazorEngine.Roslyn - Roslyn support for RazorEngine."
// !!!!!!!!!!!!!!!!!!!
// UPDATE RELEASE NOTES AS WELL!
// !!!!!!!!!!!!!!!!!!!
let version_razor4 = "4.2.2-beta1"
let version_roslyn = "3.5.4-beta1"
let version_roslyn_razor4 = "4.0.4-beta1"

// This is set to true when we want to update the roslyn packages via CI as well
// (otherwise this value doesn't matter). You can always push manually!
let roslyn_publish = System.Boolean.Parse (getBuildParamOrDefault "PUSH_ROSLYN" "false")

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
    CopyrightNotice = "RazorEngine Copyright © RazorEngine Project 2011-2015"
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
              Dependencies = [ "Microsoft.AspNet.Razor", "4.0.0-beta6" ] })
        "RazorEngine.Roslyn.nuspec", (fun config p ->
          { p with
              Project = projectName_roslyn
              Summary = projectSummary_roslyn
              Description = projectDescription_roslyn
              Version = version_roslyn
              Publish = roslyn_publish
              ReleaseNotes = toLines release.Notes
              Dependencies =
                let exact =
                  [ config.ProjectName, config.Version
                    "Microsoft.AspNet.Razor", "3.0.0" ]
                [ "Microsoft.CodeAnalysis" ]
                |> List.map (fun name -> name, (GetPackageVersion "packages" name))
                |> List.append exact })
        "RazorEngine.Roslyn-razor4.nuspec", (fun config p ->
          { p with
              Project = projectName_roslyn
              Summary = projectSummary_roslyn
              Description = projectDescription_roslyn
              Version = version_roslyn_razor4
              ReleaseNotes = toLines release.Notes
              Publish = roslyn_publish
              Dependencies =
                let exact =
                  [ config.ProjectName, version_razor4
                    "Microsoft.AspNet.Razor", "4.0.0-beta1" ]
                [ "Microsoft.CodeAnalysis" ]
                |> List.map (fun name -> name, (GetPackageVersion "packages" name))
                |> List.append exact }) ]
    UseNuget = true
    GeneratedFileList =
      [ "RazorEngine.dll"; "RazorEngine.xml"
        "RazorEngine.Roslyn.dll"; "RazorEngine.Roslyn.xml" ]
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
      let info_roslyn =
        [ Attribute.Company projectName_roslyn
          Attribute.Product projectName_roslyn
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version version_roslyn
          Attribute.FileVersion version_roslyn
          Attribute.InformationalVersion version_roslyn ]
      CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.Roslyn.cs" info_roslyn
      let info_roslyn_razor4 =
        [ Attribute.Company projectName_roslyn
          Attribute.Product projectName_roslyn
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version version_roslyn_razor4
          Attribute.FileVersion version_roslyn_razor4
          Attribute.InformationalVersion version_roslyn_razor4 ]
      CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.Roslyn-Razor4.cs" info_roslyn_razor4
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
  }

