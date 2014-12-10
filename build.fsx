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


// Supended until FAKE supports custom mono parameters
#I @".nuget/Build/FAKE/tools/" // FAKE
#r @"FakeLib.dll"  //FAKE

#load @"buildConfig.fsx"
open BuildConfig

open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile


let MyTarget name body =
    Target name (fun _ -> body false)
    let single = (sprintf "%s_single" name)
    Target single (fun _ -> body true) 

// Targets
MyTarget "Clean" (fun _ ->
    CleanDirs [ buildDir; testDir; releaseDir ]
)

MyTarget "CleanAll" (fun _ ->
    // Only done when we want to redownload all.
    Directory.EnumerateDirectories BuildConfig.nugetDir
    |> Seq.collect (fun dir -> 
        let name = Path.GetFileName dir
        if name = "Build" then
            Directory.EnumerateDirectories dir
            |> Seq.filter (fun buildDepDir ->
                let buildDepName = Path.GetFileName buildDepDir
                // We can't delete the FAKE directory (as it is used currently)
                buildDepName <> "FAKE")
        else
            Seq.singleton dir)
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
                OutputPath = BuildConfig.packageDir }))
)

MyTarget "SetVersions" (fun _ -> 
    let info =
        [Attribute.Company "RazorEngine"
         Attribute.Product "RazorEngine"
         Attribute.Copyright "Copyright © RazorEngine Project 2011-2014"
         Attribute.Version BuildConfig.version
         Attribute.FileVersion version]
    CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info
)

MyTarget "BuildApp_45" (fun _ ->
    buildApp net45Params
)

MyTarget "BuildTest_45" (fun _ ->
    buildTests net45Params
)

MyTarget "Test_45" (fun _ ->
    runTests net45Params
)

MyTarget "BuildApp_40" (fun _ ->
    buildApp net40Params
)

MyTarget "BuildTest_40" (fun _ ->
    buildTests net40Params
)

MyTarget "Test_40" (fun _ ->
    runTests net40Params
)

MyTarget "CopyToRelease" (fun _ ->
    trace "Copying to release because test was OK."
    CleanDirs [ outLibDir ]
    System.IO.Directory.CreateDirectory(outLibDir) |> ignore

    // Copy RazorEngine.dll to release directory
    [ "net40"; "net45" ] 
        |> Seq.map (fun t -> buildDir @@ t, t)
        |> Seq.filter (fun (p, t) -> Directory.Exists p)
        |> Seq.iter (fun (source, target) ->
            let outDir = outLibDir @@ target 
            ensureDirectory outDir
            [ "RazorEngine.dll"
              "RazorEngine.xml" ]
            |> Seq.iter (fun file ->
                let newFile = outDir @@ Path.GetFileName file
                File.Copy(source @@ file, newFile))
        )

    // TODO: Copy documentation
    // Zip?
    // Versioning?
)

MyTarget "NuGet" (fun _ ->
    let outDir = releaseDir @@ "nuget"
    ensureDirectory outDir
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = projectName
            Summary = projectSummary
            Description = projectDescription
            Version = release.NugetVersion
            ReleaseNotes = toLines release.Notes
            Tags = tags
            OutputPath = outDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            DependenciesByFramework =
                [ { FrameworkVersion = "net40"; 
                    Dependencies = [ "Microsoft.AspNet.Razor", "2.0.30506.0" |> RequireExactly ] }
                  { FrameworkVersion = "net45"; 
                    Dependencies = [ "Microsoft.AspNet.Razor", "3.2.2.0" ] }  ] })
        "nuget/RazorEngine.nuspec"
)

// Documentation 

MyTarget "GithubDoc" (fun _ -> buildDocumentationTarget "GithubDoc")

MyTarget "LocalDoc" (fun _ -> 
    buildDocumentationTarget "LocalDoc"
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (outDocDir @@ "local" @@ "html" @@ "index.html")))
)


MyTarget "ReleaseGithubDoc" (fun isSingle -> 
    let doAction =
        if isSingle then true
        else
            printf "update github docs? (y,n): "
            let line = System.Console.ReadLine()
            line = "y"
    if doAction then
        CleanDir "gh-pages"
        cloneSingleBranch "" (sprintf "git@github.com:%s/%s.git" github_user github_project) "gh-pages" "gh-pages"
        fullclean "gh-pages"
        CopyRecursive ("release"@@"documentation"@@(sprintf "%s.github.io" github_user)@@"html") "gh-pages" true |> printfn "%A"
        StageAll "gh-pages"
        Commit "gh-pages" (sprintf "Update generated documentation %s" release.NugetVersion)
        printf "gh-pages branch updated in the gh-pages directory, push that branch now? (y,n): "
        let line = System.Console.ReadLine()
        if line = "y" then
            Branches.pushBranch "gh-pages" "origin" "gh-pages"
)

Target "All" (fun _ ->
    trace "All finished!"
)

MyTarget "VersionBump" (fun _ ->
    // Build updates the SharedAssemblyInfo.cs files.
    let changedFiles = Fake.Git.FileStatus.getChangedFilesInWorkingCopy "" "HEAD" |> Seq.toList
    if changedFiles |> Seq.isEmpty |> not then
        for (status, file) in changedFiles do
            printfn "File %s changed (%A)" file status
        printf "version bump commit? (y,n): "
        let line = System.Console.ReadLine()
        if line = "y" then
            StageAll ""
            Commit "" (sprintf "Bump version to %s" release.NugetVersion)
            Branches.push ""
        
            printf "create tag? (y,n): "
            let line = System.Console.ReadLine()
            if line = "y" then
                Branches.tag "" release.NugetVersion
                Branches.pushTag "" "origin" release.NugetVersion
            
            printf "push branch? (y,n): "
            let line = System.Console.ReadLine()
            if line = "y" then
                Branches.push "gh-pages"
)

Target "Release" (fun _ ->
    trace "All released!"
)

// Clean all
"Clean" 
  ==> "CleanAll"
"Clean_single" 
  ==> "CleanAll_single"

// Dependencies
"Clean" 
  ==> "RestorePackages"
  ==> "SetVersions" 
  ==> "BuildApp_40"
  ==> "BuildTest_40"
  ==> "Test_40"
  ==> "BuildApp_45"
  ==> "BuildTest_45"
  ==> "Test_45"
  ==> "CopyToRelease"
  ==> "LocalDoc"
  ==> "All"
 
"All" 
  ==> "VersionBump"
  ==> "NuGet"
  ==> "GithubDoc"
  ==> "ReleaseGithubDoc"
  ==> "Release"

// start build
RunTargetOrDefault "All"
