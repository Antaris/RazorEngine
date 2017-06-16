// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
#I @"../../FAKE/tools/"
#r @"FakeLib.dll"
// NOTE: We cannot add those here because FSharp.Formatting requires Razor2
//#I @"../../FSharp.Compiler.Service/lib/net40/"
//#I @"../../Yaaf.FSharp.Scripting/lib/net40/"
//#I "../tools/"
//#r "Yaaf.AdvancedBuilding.dll"


open System.IO
open System

open Fake
open Fake.Testing.NUnit3
open Fake.MSTest
open AssemblyInfoFile

(**
## The `BuildParams` Type

You can define your own type for building, the only limitation is that this type needs the `SimpleBuildName` and `CustomBuildName` properties.
The `SimpleBuildName` property is used for the generated FAKE target for this build `(sprintf "Build_%s" build.SimpleBuildName)`.
The `CustomBuildName` is used as a parameter for msbuild/xbuild and can be used within the fsproj and csproj files to define custom builds.
(IE. custom defines / targets and so on).
The `CustomBuildName` property is also used as the name of the sub-directory within the `buildDir` (see below).
*)

type BuildParams =
  { /// The name of the output folder and the build target
    SimpleBuildName : string
    /// The name of the platform to build
    PlatformName : string
    BuildMode : string
    DisableProjectFileCreation : bool
    UseProjectOutDir : bool
    BeforeBuild : unit -> unit
    AfterBuild : unit -> unit
    AfterTest : unit -> unit
    FindSolutionFiles : BuildParams -> string seq 
    FindProjectFiles : BuildParams -> string seq
    FindTestFiles : BuildParams -> string seq
    FindUnitTestDlls : (string * BuildParams) -> string seq }
  static member Empty = 
    { SimpleBuildName = ""
      BuildMode = "Release"
      BeforeBuild = fun _ -> ()
      AfterBuild = fun _ -> ()
      AfterTest = fun _ -> ()
      PlatformName = "AnyCPU"
      DisableProjectFileCreation = false
      UseProjectOutDir = false
      FindSolutionFiles = fun _ -> Seq.empty
      FindProjectFiles = fun (_:BuildParams) ->
        !! (sprintf "src/**/*.fsproj")
        ++ (sprintf "src/**/*.csproj")
        -- (sprintf "src/**/*.Tests.fsproj")
        -- (sprintf "src/**/*.Tests.csproj")
        -- (sprintf "src/**/Test.*.fsproj")
        -- (sprintf "src/**/Test.*.csproj")
        :> _
      FindTestFiles = fun (_:BuildParams) ->
        !! (sprintf "src/**/*.Tests.fsproj")
        ++ (sprintf "src/**/*.Tests.csproj")
        ++ (sprintf "src/**/Test.*.fsproj")
        ++ (sprintf "src/**/Test.*.csproj")
        :> _
      FindUnitTestDlls = fun (testDir, (_:BuildParams)) ->
        !! (testDir + "/Test.*.dll")
        ++ (testDir + "/*.Tests.dll")
        :> _ }
  static member WithSolution =
   { BuildParams.Empty with
      BuildMode = "Release"
      PlatformName = "Any CPU"
      UseProjectOutDir = true
      FindSolutionFiles = fun _ -> !! "**/*.sln" :> _
      FindProjectFiles = fun _ -> Seq.empty
      FindTestFiles = fun _ -> Seq.empty }

/// see http://tpetricek.github.io/FSharp.Formatting/diagnostics.html
type FSharpFormattingLogging =
  /// Disable all logging. F# Formatting will not print anything to the console and it will also not produce a log file (this is not recomended, but you might need this if you want to suppress all output).
  | DisableFSFLogging
  /// Enables detailed logging to a file FSharp.Formatting.svclog and keeps printing of basic information to console too.
  | AllFSFLogging
  /// Enables detailed logging to a file FSharp.Formatting.svclog but disables printing of basic information to console.
  | FileOnlyFSFLogging
  /// Any other value (default) - Print basic information to console and do not produce a detailed log file.
  | ConsoleOnlyFSFLogging

type NuGetPackage =
  { /// The version of the package (null = use global version)
    Version : string
    /// The filename of the nuspec (template) file, in the config.NugetDir folder
    FileName : string
    /// The prefix for the created tag (null = no tag will be created)
    TagPrefix : string
    /// identifier for this package
    SimpleName : string

    ConfigFun : (NuGetParams -> NuGetParams) } 
  static member Empty =
    { Version = null
      FileName = null
      TagPrefix = null
      SimpleName = null
      ConfigFun = id }
  member x.Name =
    if isNull x.SimpleName then x.TagPrefix else x.SimpleName
  member x.TagName =
    if isNull x.TagPrefix then failwith "no TagPrefix is specified!"
    sprintf "%s%s" x.TagPrefix x.Version
  member x.VersionLine =
    sprintf "%s_%s" x.SimpleName (if isNull x.TagPrefix then x.Version else x.TagName)

type BuildConfiguration =
  { // Metadata
    ProjectName : string
    ProjectSummary : string
    Company : string
    CopyrightNotice : string
    ProjectDescription : string
    ProjectAuthors : string list
    /// Enable all github integrations (pushing documentation)
    EnableGithub : bool
    GithubUser : string
    /// Defaults to ProjectName
    GithubProject : string
    PageAuthor : string
    /// Defaults to github issues
    IssuesUrl : string
    /// Defaults to github new issue page
    FileNewIssueUrl : string
    /// Defaults to github master branch "/blob/master/"
    SourceReproUrl : string

    // Nuget configuration
    /// Defaults to sprintf "https://www.nuget.org/packages/%s/" x.ProjectName
    NugetUrl : string
    NugetTags : string
    /// The directory for the nuspec (template) files.
    NugetDir : string
    /// Like NugetPackages but allows to define different versions (which will create tags)
    NugetVersionPackages : BuildConfiguration -> NuGetPackage list
    // [<Obsolete("Use NugetVersionPackages instead")>] 
    // see https://fslang.uservoice.com/forums/245727-f-language/suggestions/12826233-hide-obsolete-warnings-on-record-initializer-not-u
    NugetPackages : (string * (BuildConfiguration -> NuGetParams -> NuGetParams)) list
    // Defaults to "./release/nuget/"
    OutNugetDir : string

    // Pre build
    Version : string
    /// Defaults to setting up a "./src/SharedAssemblyInfo.fs" and "./src/SharedAssemblyInfo.cs"
    SetAssemblyFileVersions : BuildConfiguration -> unit
    /// Enables to convert pdb to mdb or mdb to pdb after paket restore.
    /// This improves cross platform development and creates pdb files 
    /// on unix (to create nuget packages on linux with integrated pdb files)
    EnableDebugSymbolConversion : bool

    /// Makes "./build.sh Release" fail when not executed on a windows machine
    /// Use this if you want to include .pdb in your nuget packge 
    /// (to ensure your release contains debug symbols)
    RestrictReleaseToWindows : bool

    // Build configuration
    /// Defaults to [ x.ProjectName + ".dll"; x.ProjectName + ".xml" ]
    GeneratedFileList : string list
    /// Defaults to false (support for nuget msbuild integration)
    UseNuget : bool
    BuildTargets : BuildParams list
    /// Defaults to "./build/"
    BuildDir : string
    /// Defaults to "./release/lib/"
    OutLibDir : string
    NugetPackageDir : string
    GlobalPackagesDir : string

    // Test
    /// Defaults to "./test/"
    TestDir : string

    DisableNUnit : bool
    SetupNUnit : (NUnitParams -> NUnitParams)

    DisableNUnit3 : bool
    SetupNUnit3 : (NUnit3Params -> NUnit3Params)

    DisableMSTest : bool
    SetupMSTest : (MSTestParams -> MSTestParams)

    // Documentation generation
    /// Defaults to "./release/documentation/"
    OutDocDir : string
    /// Defaults to "./doc/templates/"
    DocTemplatesDir : string
    DocLogging : FSharpFormattingLogging
    LayoutRoots : string list
    /// Specify the list of references used for (razor) documentation generation.
    DocRazorReferences : string list option }
  static member Defaults =
    { ProjectName = ""
      ProjectSummary = ""
      Company = ""
      CopyrightNotice = ""
      ProjectDescription = ""
      UseNuget = false
      EnableGithub = true
      EnableDebugSymbolConversion = false
      RestrictReleaseToWindows = true
      ProjectAuthors = []
      BuildTargets = [ BuildParams.Empty ]
      NugetUrl = ""
      NugetTags = ""
      PageAuthor = ""
      GithubUser = ""
      GithubProject = ""
      DocLogging = AllFSFLogging
      SetAssemblyFileVersions = (fun config ->
        let info =
          [ Attribute.Company config.Company
            Attribute.Product config.ProjectName
            Attribute.Copyright config.CopyrightNotice
            Attribute.Version config.Version
            Attribute.FileVersion config.Version
            Attribute.InformationalVersion config.Version ]
        CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info
        CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info)
      Version = ""
      IssuesUrl = ""
      FileNewIssueUrl = ""
      SourceReproUrl = ""
      NugetDir = "nuget"
      NugetVersionPackages = fun _ -> []
      NugetPackages = []
      DisableNUnit = false
      SetupNUnit = id
      DisableNUnit3 = false
      SetupNUnit3 = id
      DisableMSTest = isLinux
      SetupMSTest = id
      GeneratedFileList = []
      BuildDir = "./build/"
      OutLibDir = "./release/lib/"
      OutDocDir = "./release/documentation/"
      OutNugetDir = "./release/nuget/"
      DocTemplatesDir = "./doc/templates/"
      LayoutRoots = [ ]
      TestDir  = "./build/test/"
      GlobalPackagesDir = "./packages"
      NugetPackageDir = "./packages/.nuget"
      DocRazorReferences =
        if isMono then
          let loadedList =
            System.AppDomain.CurrentDomain.GetAssemblies()
            |> Seq.choose (fun a -> try Some (a.Location) with _ -> None)
            |> Seq.cache
          let getItem name = loadedList |> Seq.find (fun l -> l.Contains name)
          [ (getItem "FSharp.Core").Replace("4.3.0.0", "4.3.1.0")  // (if isMono then "/usr/lib64/mono/gac/FSharp.Core/4.3.1.0__b03f5f7f11d50a3a/FSharp.Core.dll" else "FSharp.Core") 
            Path.GetFullPath "./packages/FSharp.Compiler.Service/lib/net40/FSharp.Compiler.Service.dll"
            Path.GetFullPath "./packages/FSharp.Formatting/lib/net40/System.Web.Razor.dll"
            Path.GetFullPath "./packages/FSharp.Formatting/lib/net40/RazorEngine.dll"
            Path.GetFullPath "./packages/FSharp.Formatting/lib/net40/FSharp.Literate.dll"
            Path.GetFullPath "./packages/FSharp.Formatting/lib/net40/FSharp.CodeFormat.dll"
            Path.GetFullPath "./packages/FSharp.Formatting/lib/net40/FSharp.MetadataFormat.dll" ]
          |> Some
        else None}
  member x.GithubUrl = sprintf "https://github.com/%s/%s" x.GithubUser x.GithubProject
  member x.AllNugetPackages =
    let versionPackages = x.NugetVersionPackages x
    let otherPackages = x.NugetPackages |> List.map (fun (s, func) -> { NuGetPackage.Empty with FileName = s; ConfigFun = func x })
    versionPackages @ otherPackages
  member x.GetPackageByName name =
    x.AllNugetPackages |> Seq.find (fun p -> p.Name = name)
  member x.SpecialVersionPackages =
    x.AllNugetPackages |> List.filter (fun p -> not (isNull p.Version))
  member x.VersionInfoLine =
    let packages = x.SpecialVersionPackages
    if packages.Length = 0 then
      x.Version
    else
      sprintf "%s (%s)" x.Version (String.Join(", ", packages |> Seq.map (fun p -> p.VersionLine)))
  member x.CheckValid() =
    match x.AllNugetPackages |> Seq.tryFind (fun p -> isNull p.FileName) with
    | Some p ->
      failwithf "found a package with a FileName of null: %A" p
    | None -> ()
    let packages = x.SpecialVersionPackages
    match packages |> Seq.tryFind (fun p -> isNull p.Name) with
    | Some p ->
      failwithf "package '%s' has a version '%s' but SimpleName and TagPrefix are both null!" p.FileName p.Version
    | None -> ()
    match x.AllNugetPackages |> Seq.tryFind (fun p -> isNull p.Version && not (isNull p.TagPrefix)) with
    | Some p ->
      failwithf "package '%s' has a TagPrefix set but it's version is not set (eg it is null)" p.FileName
    | None -> ()

  member x.FillDefaults () =
    let x =
      { x with
          Company =
            if String.IsNullOrEmpty x.Company then x.ProjectName else x.Company
          NugetUrl =
            if String.IsNullOrEmpty x.NugetUrl then sprintf "https://www.nuget.org/packages/%s/" x.ProjectName else x.NugetUrl
          GithubProject = if String.IsNullOrEmpty x.GithubProject then x.ProjectName else x.GithubProject
          GeneratedFileList =
            if x.GeneratedFileList |> List.isEmpty |> not then x.GeneratedFileList
            else [ x.ProjectName + ".dll"; x.ProjectName + ".xml" ]
          LayoutRoots =
            if not x.LayoutRoots.IsEmpty then x.LayoutRoots
            else [ x.DocTemplatesDir; x.DocTemplatesDir @@ "reference" ] }
    // GithubUrl is now available
    let final =
      { x with
            SourceReproUrl =
              if String.IsNullOrEmpty x.SourceReproUrl then x.GithubUrl + "/blob/master/" else x.SourceReproUrl
            IssuesUrl = if String.IsNullOrEmpty x.IssuesUrl then sprintf "%s/issues" x.GithubUrl else x.IssuesUrl
            FileNewIssueUrl =
              if String.IsNullOrEmpty x.FileNewIssueUrl then sprintf "%s/issues/new" x.GithubUrl else x.FileNewIssueUrl }
    final.CheckValid()
    final