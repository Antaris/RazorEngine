// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the complete build process of RazorEngine

    The first step is handled in build.sh and build.cmd by bootstrapping a NuGet.exe and 
    executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE)

    The secound step is executing this file which resolves all dependencies, builds the solution and executes all unit tests
*)

#if FAKE
#else
// Support when file is opened in Visual Studio
#load "buildConfigDef.fsx"
#load "../../../buildConfig.fsx"
#endif

open BuildConfigDef
let config = BuildConfig.buildConfig.FillDefaults ()

// NOTE: We want to add that to buildConfigDef.fsx sometimes in the future
// #I @"../../FSharp.Compiler.Service/lib/net40/" // included in FAKE, but to be able to use the latest
// Bundled
//#I @"../../Yaaf.FSharp.Scripting/lib/net40/"
#I "../tools/"
#r "Yaaf.AdvancedBuilding.dll"


open Yaaf.AdvancedBuilding
open System.IO
open System

open Fake
open Fake.Testing.NUnit3
open Fake.Git
open Fake.MSTest
open Fake.FSharpFormatting
open AssemblyInfoFile

let readLine msg def =
  if isLocalBuild then
    printf "%s" msg
    System.Console.ReadLine()
  else
    printf "%s%s" msg def
    def

if config.UseNuget then
    // Ensure the ./src/.nuget/NuGet.exe file exists (required by xbuild)
    let nuget = findToolInSubPath "NuGet.exe" (config.GlobalPackagesDir @@ "NuGet.CommandLine/tools/NuGet.exe")
    if Directory.Exists "./src/.nuget" then
      File.Copy(nuget, "./src/.nuget/NuGet.exe", true)
    else
      failwith "you set UseNuget to true but there is no \"./src/.nuget/NuGet.targets\" or \"./src/.nuget/NuGet.Config\"! Please copy them from ./packages/Yaaf.AdvancedBuilding/scaffold/nuget"

let createMissingSymbolFiles assembly =
  try
    match File.Exists (Path.ChangeExtension(assembly, "pdb")), File.Exists (assembly + ".mdb") with
    | true, false when not isLinux ->
      // create mdb
      trace (sprintf "Creating mdb for %s" assembly)
      DebugSymbolHelper.writeMdbFromPdb assembly
    | true, false ->
      trace (sprintf "Cannot create mdb for %s because we are not on windows :(" assembly) 
    | false, true when not isLinux ->
      // create pdb
      trace (sprintf "Creating pdb for %s" assembly)
      DebugSymbolHelper.writePdbFromMdb assembly
    | false, true ->
      trace (sprintf "Cannot create pdb for %s because we are not on windows :(" assembly) 
    | _, _ -> 
      // either no debug symbols available or already both.
      ()
  with exn -> traceError (sprintf "Error creating symbols: %s" exn.Message)


let buildWithFiles msg dir projectFileFinder (buildParams:BuildParams) =
    let files = projectFileFinder buildParams |> Seq.toList
    let buildDir = 
        if not buildParams.UseProjectOutDir then
            let buildDir = dir @@ buildParams.SimpleBuildName
            CleanDirs [ buildDir ]
            buildDir
        else 
            files
                |> MSBuild null "clean"
                    [ "Configuration", buildParams.BuildMode
                      "Platform", buildParams.PlatformName ] 
                |> Log "Cleaning: "
            null
    // build app
    files
        |> MSBuild buildDir "Build"
            [ "Configuration", buildParams.BuildMode
              "Platform", buildParams.PlatformName ]
        |> Log msg
        
let buildSolution = buildWithFiles "BuildSolution-Output: " config.BuildDir (fun buildParams -> buildParams.FindSolutionFiles buildParams)
let buildApp = buildWithFiles "AppBuild-Output: " config.BuildDir (fun buildParams -> buildParams.FindProjectFiles buildParams)
let buildTests = buildWithFiles "TestBuild-Output: " config.TestDir (fun buildParams -> buildParams.FindTestFiles buildParams)

exception NUnitNotFoundException of string

let runTests (buildParams:BuildParams) =
    let testDir = config.TestDir @@ buildParams.SimpleBuildName
    let logs = System.IO.Path.Combine(testDir, "logs")
    System.IO.Directory.CreateDirectory(logs) |> ignore
    let files = buildParams.FindUnitTestDlls (testDir, buildParams) |> Seq.cache
    if files |> Seq.isEmpty then
      traceError (sprintf "NO test found in %s" testDir)
    else
      let legacyNunitRun =
        if not config.DisableNUnit then
          try
            files
              |> NUnit (fun p ->
                  let setupValue =
                    {p with
                      ProcessModel =
                          // Because the default nunit-console.exe.config doesn't use .net 4...
                          if isMono then NUnitProcessModel.SingleProcessModel else NUnitProcessModel.DefaultProcessModel
                      WorkingDir = testDir
                      StopOnError = true
                      TimeOut = System.TimeSpan.FromMinutes 30.0
                      Framework = "4.0"
                      DisableShadowCopy = true
                      OutputFile = "logs/TestResults.xml" } |> config.SetupNUnit
                  let tool = setupValue.ToolPath @@ setupValue.ToolName
                  if File.Exists tool |> not then
                    raise <| NUnitNotFoundException (sprintf "The path to the nunit runner (%s) was not found!\nIt might be because you updated NUnit and they changed the path to the executable.\nEither downgrade NUnit again or use the new API (if already available)." tool)
                  setupValue)
            true
          with
          | NUnitNotFoundException s ->
            traceEndTask "NUnit" (files |> separated ", ") // Workaround for https://github.com/fsharp/FAKE/issues/1079
            let msg = sprintf "NUNIT COULD NOT BE RUN, because it was not found. Please disable NUnit in your buildConfigDef.fsx with 'DisableNUnit = true'.\n\nDetails: %s" s
            if not config.DisableNUnit3 && File.Exists Fake.Testing.NUnit3.NUnit3Defaults.ToolPath then
              traceFAKE "%s\n\nThis is a warning only because we will be running NUnit3 as well" msg
            else failwith msg
            false
        else false
      if not legacyNunitRun && not config.DisableNUnit3 then
        files
          |> NUnit3 (fun p ->
              let setupValue =
                {p with
                  ProcessModel =
                      // Because the default nunit-console.exe.config doesn't use .net 4...
                      if isMono then NUnit3ProcessModel.SingleProcessModel else NUnit3ProcessModel.DefaultProcessModel
                  WorkingDir = testDir
                  StopOnError = true
                  TimeOut = System.TimeSpan.FromMinutes 30.0
                  Framework = if isMono then NUnit3Runtime.Mono40 else NUnit3Runtime.Net45
                  ShadowCopy = false
                  OutputDir = "logs" } |> config.SetupNUnit3
              let tool = setupValue.ToolPath
              if File.Exists tool |> not then
                failwithf "The path to the nunit runner (%s) was not found!\nIt might be because you updated NUnit and they changed the path to the executable.\nEither downgrade NUnit again or use the new API (if already available)." tool
              setupValue)

      if not config.DisableMSTest then
        files
          |> MSTest (fun p ->
              {p with
                  WorkingDir = testDir
                  ResultsDir = "logs" } |> config.SetupMSTest)

let buildAll (buildParams:BuildParams) =
    buildParams.BeforeBuild ()
    buildSolution buildParams
    buildApp buildParams
    buildTests buildParams
    buildParams.AfterBuild ()
    runTests buildParams
    buildParams.AfterTest ()

let fakePath = "packages" @@ "FAKE" @@ "tools" @@ "FAKE.exe"
let fakeStartInfo script workingDirectory args environmentVars =
    (fun (info: System.Diagnostics.ProcessStartInfo) ->
        info.FileName <- fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE \"%s\"" args script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)


/// Run the given startinfo by printing the output (live)
let executeWithOutput configStartInfo =
    let exitCode =
        ExecProcessWithLambdas
            configStartInfo
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

/// Run the given startinfo by redirecting the output (live)
let executeWithRedirect errorF messageF configStartInfo =
    let exitCode =
        ExecProcessWithLambdas
            configStartInfo
            TimeSpan.MaxValue true errorF messageF
    System.Threading.Thread.Sleep 1000
    exitCode

/// Helper to fail when the exitcode is <> 0
let executeHelper executer traceMsg failMessage configStartInfo =
    trace traceMsg
    let exit = executer configStartInfo
    if exit <> 0 then
        failwith failMessage
    ()

let execute = executeHelper executeWithOutput

/// Undocumentated way to disable cache (-nc) for documentation generation
let mutable documentationFAKEArgs = ""

// Documentation
let buildDocumentationTarget target =
    let loggingValue =
      match config.DocLogging with
      | DisableFSFLogging -> "NONE"
      | AllFSFLogging -> "ALL"
      | FileOnlyFSFLogging -> "FILE_ONLY"
      | ConsoleOnlyFSFLogging -> "CONSOLE_ONLY"
    execute
      (sprintf "Building documentation (%s), this could take some time, please wait..." target)
      "generating reference documentation failed"
      (fakeStartInfo "generateDocs.fsx" "." documentationFAKEArgs ["target", target; "FSHARP_FORMATTING_LOG", loggingValue])

let tryDelete dir =
    try
        CleanDirs [ dir ]
    with
    | :? System.IO.IOException as e ->
        traceImportant (sprintf "Cannot access: %s\nTry closing Visual Studio!" e.Message)
    | :? System.UnauthorizedAccessException as e ->
        traceImportant (sprintf "Cannot access: %s\nTry closing Visual Studio!" e.Message)

let MyTarget name body =
    Target name (fun _ -> body false)
    let single = (sprintf "%s_single" name)
    Target single (fun _ -> body true)

// Targets
MyTarget "Clean" (fun _ ->
    tryDelete config.BuildDir
    tryDelete config.TestDir

    CleanDirs [ config.BuildDir; config.TestDir; config.OutLibDir; config.OutDocDir; config.OutNugetDir ]
)

MyTarget "CleanAll" (fun _ ->
    // Only done when we want to redownload all.
    Directory.EnumerateDirectories config.GlobalPackagesDir
    |> Seq.filter (fun buildDepDir ->
        let buildDepName = Path.GetFileName buildDepDir
        // We can't delete the FAKE directory (as it is used currently)
        buildDepName <> "FAKE")
    |> Seq.iter (fun dir ->
        try
            DeleteDir dir
        with exn ->
            traceError (sprintf "Unable to delete %s: %O" dir exn))
)

MyTarget "RestorePackages" (fun _ ->
    // will catch src/targetsDependencies
    !! "./src/**/packages.config"
    |> Seq.iter 
        (RestorePackage (fun param ->
            { param with    
                // ToolPath = ""
                OutputPath = config.NugetPackageDir }))
)

MyTarget "CreateDebugFiles" (fun _ ->
    // creates .mdb from .pdb files and the other way around
    !! (config.GlobalPackagesDir + "/**/*.exe")
    ++ (config.GlobalPackagesDir + "/**/*.dll")
    |> Seq.iter createMissingSymbolFiles  
)

MyTarget "SetVersions" (fun _ -> 
    config.SetAssemblyFileVersions config
)

config.BuildTargets
    |> Seq.iter (fun buildParam -> 
        MyTarget (sprintf "Build_%s" buildParam.SimpleBuildName) (fun _ -> buildAll buildParam))

MyTarget "CopyToRelease" (fun _ ->
    trace "Copying to release because test was OK."
    let outLibDir = config.OutLibDir
    CleanDirs [ outLibDir ]
    Directory.CreateDirectory(outLibDir) |> ignore

    // Copy files to release directory
    config.BuildTargets
        |> Seq.map (fun buildParam -> buildParam.SimpleBuildName)
        |> Seq.map (fun t -> config.BuildDir @@ t, t)
        |> Seq.filter (fun (p, _) -> Directory.Exists p)
        |> Seq.iter (fun (source, buildName) ->
            let outDir = outLibDir @@ buildName
            ensureDirectory outDir
            config.GeneratedFileList
            |> Seq.collect (fun file ->
              let extension = (Path.GetExtension file).TrimStart('.')
              match extension with
              | "dll" | "exe" -> 
                [ file
                  Path.ChangeExtension(file, "pdb")
                  Path.ChangeExtension(file, extension + ".mdb" ) ]              
              | _ -> [ file ]
            )
            |> Seq.filter (fun (file) -> File.Exists (source @@ file))
            |> Seq.iter (fun (file) ->
                let sourceFile = source @@ file
                let newfile = outDir @@ Path.GetFileName file
                trace (sprintf "Copying %s to %s" sourceFile newfile)
                File.Copy(sourceFile, newfile))
        )
)

MyTarget "CreateReleaseSymbolFiles" (fun _ ->
    // creates .mdb from .pdb files and the other way around
    !! (config.OutLibDir + "/**/*.exe")
    ++ (config.OutLibDir + "/**/*.dll")
    |> Seq.iter createMissingSymbolFiles  
)

/// push package (and try again if something fails), FAKE Version doesn't work on mono
/// From https://raw.githubusercontent.com/fsharp/FAKE/master/src/app/FakeLib/NuGet/NugetHelper.fs
let rec private publish parameters =
    let replaceAccessKey key (text : string) =
        if isNullOrEmpty key then text
        else text.Replace(key, "PRIVATEKEY")
    let nuspec = sprintf "%s.%s.nupkg" parameters.Project parameters.Version
    traceStartTask "MyNuGetPublish" nuspec
    let tracing = enableProcessTracing
    enableProcessTracing <- false
    let source =
        let uri = if isNullOrEmpty parameters.PublishUrl then "https://www.nuget.org/api/v2/package" else parameters.PublishUrl
        sprintf "-s %s" uri

    let args = sprintf "push \"%s\" %s %s" (parameters.OutputPath @@ nuspec) parameters.AccessKey source
    tracefn "%s %s in WorkingDir: %s Trials left: %d" parameters.ToolPath (replaceAccessKey parameters.AccessKey args)
        (FullName parameters.WorkingDir) parameters.PublishTrials
    try
      try
        let result =
            ExecProcess (fun info ->
                info.FileName <- parameters.ToolPath
                info.WorkingDirectory <- FullName parameters.WorkingDir
                info.Arguments <- args) parameters.TimeOut
        enableProcessTracing <- tracing
        if result <> 0 then failwithf "Error during NuGet push. %s %s" parameters.ToolPath args
        true
      with exn ->
        let existsError = exn.Message.Contains("already exists and cannot be modified")
        if existsError then
          trace exn.Message
          false
        else
          if parameters.PublishTrials > 0 then publish { parameters with PublishTrials = parameters.PublishTrials - 1 }
          else
            (if not (isNull exn.InnerException) then exn.Message + "\r\n" + exn.InnerException.Message
             else exn.Message)
            |> replaceAccessKey parameters.AccessKey
            |> failwith
    finally
      traceEndTask "MyNuGetPublish" nuspec

let packSetup version config p =
  { p with
      Authors = config.ProjectAuthors
      Project = config.ProjectName
      Summary = config.ProjectSummary
      Version = if isNull version then config.Version else version
      Description = config.ProjectDescription
      Tags = config.NugetTags
      WorkingDir = "."
      OutputPath = config.OutNugetDir
      AccessKey = getBuildParamOrDefault "nugetkey" ""
      Publish = false
      Dependencies = [ ] }

MyTarget "NuGetPack" (fun _ ->
    ensureDirectory config.OutNugetDir
    for package in config.AllNugetPackages do
      let packSetup = packSetup package.Version config
      NuGet (fun p -> { (packSetup >> package.ConfigFun) p with Publish = false }) (Path.Combine(config.NugetDir, package.FileName))
)

MyTarget "NuGetPush" (fun _ ->
    let packagePushed =
      config.AllNugetPackages 
      |> List.map (fun package ->
        try
          let packSetup = packSetup package.Version config
          let parameters = NuGetDefaults() |> (fun p -> { packSetup p with Publish = true }) |> package.ConfigFun
          // This allows us to specify packages which we do not want to push...
          if hasBuildParam "nugetkey" && parameters.Publish then publish parameters
          else true
        with e -> 
          trace (sprintf "Could not push package '%s': %O" (if isNull package.Name then "{null}" else package.Name) e)
          false)
      |> List.exists id

    if not packagePushed then
      failwithf "No package could be pushed!"
)

// Documentation 
MyTarget "GithubDoc" (fun _ -> buildDocumentationTarget "GithubDoc")

MyTarget "LocalDoc" (fun _ -> buildDocumentationTarget "LocalDoc")

MyTarget "WatchDocs" (fun _ -> buildDocumentationTarget "WatchDocs")

// its just faster to generate all at the same time...
MyTarget "AllDocs" (fun _ -> buildDocumentationTarget "AllDocs")

MyTarget "ReleaseGithubDoc" (fun isSingle ->
    let repro = (sprintf "git@github.com:%s/%s.git" config.GithubUser config.GithubProject)
    let doAction =
        if isSingle then true
        else
            let msg = sprintf "update github docs to %s? (y,n): " repro
            let line = readLine msg "y"
            line = "y"
    if doAction then
        CleanDir "gh-pages"
        cloneSingleBranch "" repro "gh-pages" "gh-pages"
        fullclean "gh-pages"
        CopyRecursive ("release"@@"documentation"@@(sprintf "%s.github.io" config.GithubUser)@@"html") "gh-pages" true |> printfn "%A"
        StageAll "gh-pages"
        Commit "gh-pages" (sprintf "Update generated documentation %s" config.VersionInfoLine)
        let msg = sprintf "gh-pages branch updated in the gh-pages directory, push that branch to %s now? (y,n): " repro
        let line = readLine msg "y"
        if line = "y" then
            Branches.pushBranch "gh-pages" "origin" "gh-pages"
)

Target "All" (fun _ ->
    trace "All finished!"
)

Target "CheckWindows" (fun _ ->
    if isLinux then failwith "can only do releases on windows." 
)

MyTarget "VersionBump" (fun _ ->
    let repositoryHelperDir =  "__repository"
    let workingDir =
      if not isLocalBuild && buildServer = BuildServer.TeamFoundation then
        let workingDir = repositoryHelperDir
        // We are not in a git repository, because the .git folder is missing.
        let repro = (sprintf "git@github.com:%s/%s.git" config.GithubUser config.GithubProject)
        CleanDir workingDir
        clone "" repro workingDir
        checkoutBranch workingDir (Information.getCurrentSHA1("."))
        fullclean (workingDir @@ "src")
        CopyRecursive "src" (workingDir @@ "src") true |> printfn "%A"
        workingDir
      else ""
      
    let doBranchUpdates = not isLocalBuild && (getBuildParamOrDefault "yaaf_merge_master" "false") = "true"
    if doBranchUpdates then
      // Make sure we are on develop (commit will fail otherwise)
      Stash.push workingDir ""
      try Branches.deleteBranch workingDir true "develop"
      with e -> trace (sprintf "deletion of develop branch failed %O" e)
      Branches.checkout workingDir true "develop"
      try Stash.pop workingDir
      with e -> trace (sprintf "stash pop failed %O" e)

    // Commit updates the SharedAssemblyInfo.cs files.
    let changedFiles = Fake.Git.FileStatus.getChangedFilesInWorkingCopy workingDir "HEAD" |> Seq.toList
    if changedFiles |> Seq.isEmpty |> not then
        for (status, file) in changedFiles do
            printfn "File %s changed (%A)" file status

        let line = readLine "version bump commit? (y,n): " "y"
        if line = "y" then
            StageAll workingDir
            Commit workingDir (sprintf "Bump version to %s" config.VersionInfoLine)

    if doBranchUpdates then
      try Branches.deleteBranch workingDir true "master"
      with e -> trace (sprintf "deletion of master branch failed %O" e)
      Branches.checkout workingDir false "origin/master"
      Branches.checkout workingDir true "master"
      try Merge.merge workingDir NoFastForwardFlag "develop"
      with e ->
        trace (sprintf "merging of develop into master failed: %O" e)
        trace (sprintf "Try 'git checkout develop && git pull origin master && git checkout master && git pull origin master && git merge develop && git push origin master' locally and repeat the release process!")
        reraise()
        
      //try Branches.deleteTag "" config.Version
      //with e -> trace (sprintf "deletion of tag %s failed %O" config.Version e)
      
      let specialVersionedPackages =
        config.SpecialVersionPackages
        |> List.filter (fun p -> not (isNull p.TagPrefix))
      let createdPackageTags =
        specialVersionedPackages 
          |> List.map (fun p ->
            try Branches.tag workingDir p.TagName; true
            with e ->  trace (sprintf "creation of tag '%s' failed: %O" p.TagName e); false)
          |> List.exists id
      
      try Branches.tag workingDir config.Version
      with e ->
        if createdPackageTags then
          trace (sprintf "creation of tag '%s' failed: %O" config.Version e)
        else
          raise <| new Exception("No tag was created for this release, please increase a (package) version!", e)
      
      try Branches.deleteBranch workingDir true "develop"
      with e -> trace (sprintf "deletion of develop branch failed %O" e)
      Branches.checkout workingDir false "origin/develop"
      Branches.checkout workingDir true "develop"
      try Merge.merge workingDir NoFastForwardFlag "master"
      with e ->
        trace (sprintf "merging of master into develop failed: %O" e)
        trace (sprintf "Try 'git checkout master && git pull origin master && git checkout develop && git pull origin master && git merge master && git push origin develop' locally and repeat the release process!")
        reraise()

      for p in specialVersionedPackages do
        Branches.pushTag workingDir "origin" p.TagName
      Branches.pushTag workingDir "origin" config.Version
      Branches.pushBranch workingDir "origin" "develop"
      Branches.pushBranch workingDir "origin" "master"
    CleanDir repositoryHelperDir
    DeleteDir repositoryHelperDir
)

Target "Release" (fun _ ->
    trace "All released!"
)

Target "ReadyForBuild" ignore
Target "AfterBuild" ignore

// Clean all
"Clean" 
  ==> "CleanAll"
"Clean_single" 
  ==> "CleanAll_single"

"Clean"
  =?> ("RestorePackages", config.UseNuget)
  =?> ("CreateDebugFiles", config.EnableDebugSymbolConversion)
  ==> "SetVersions"
  ==> "ReadyForBuild"

config.BuildTargets
    |> Seq.iter (fun buildParam ->
        let buildName = sprintf "Build_%s" buildParam.SimpleBuildName
        "ReadyForBuild"
          ==> buildName
          |> ignore
        buildName
          ==> "AfterBuild"
          |> ignore
    )

// Dependencies
"AfterBuild" 
  ==> "CopyToRelease"
  =?> ("CreateReleaseSymbolFiles", config.EnableDebugSymbolConversion)
  ==> "NuGetPack"
  ==> "AllDocs"
  ==> "All"
 
"All"
  =?> ("CheckWindows", config.RestrictReleaseToWindows)
  ==> "VersionBump"
  =?> ("ReleaseGithubDoc", config.EnableGithub)
  ==> "NuGetPush"
  ==> "Release"

