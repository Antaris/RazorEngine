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
         Attribute.Version version
         Attribute.FileVersion version
         Attribute.InformationalVersion version_nuget]
    CreateCSharpAssemblyInfo "./src/SharedAssemblyInfo.cs" info
)

MyTarget "SetVersions_Razor4" (fun _ -> 
    let info =
        [Attribute.Company "RazorEngine"
         Attribute.Product "RazorEngine"
         Attribute.Copyright "Copyright © RazorEngine Project 2011-2014"
         Attribute.Version version_razor4
         Attribute.FileVersion version_razor4
         Attribute.InformationalVersion version_razor4_nuget]
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

MyTarget "BuildApp_Razor4" (fun _ ->
    buildApp razor4Params
)

MyTarget "BuildTest_Razor4" (fun _ ->
    buildTests razor4Params
)

MyTarget "Test_Razor4" (fun _ ->
    runTests razor4Params
)

MyTarget "CopyToRelease" (fun _ ->
    trace "Copying to release because test was OK."
    CleanDirs [ outLibDir ]
    System.IO.Directory.CreateDirectory(outLibDir) |> ignore

    // Copy RazorEngine.dll to release directory
    [ "net40"; "net45"; "razor4" ] 
        |> Seq.map (fun t -> buildDir @@ t, t)
        |> Seq.filter (fun (p, t) -> Directory.Exists p)
        |> Seq.iter (fun (source, target) ->
            let outDir = outLibDir @@ target 
            ensureDirectory outDir
            [ "RazorEngine.dll"
              "RazorEngine.xml"
              "RazorEngine.Roslyn.dll"
              "RazorEngine.Roslyn.xml" ]
            |> Seq.filter (fun (file) -> File.Exists (source @@ file))
            |> Seq.iter (fun (file) ->
                let newfile = outDir @@ Path.GetFileName file
                File.Copy(source @@ file, newfile))
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
            Version = version_nuget
            ReleaseNotes = toLines release.Notes
            Tags = tags
            OutputPath = outDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            DependenciesByFramework =
                [ { FrameworkVersion = "net40"; 
                    Dependencies = [ "Microsoft.AspNet.Razor", "2.0.30506.0" |> RequireExactly ] }
                  { FrameworkVersion = "net45"; 
                    Dependencies = [ "Microsoft.AspNet.Razor", "3.0.0" ] }  ] })
        "nuget/RazorEngine.nuspec"
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = projectName
            Summary = projectSummary
            Description = projectDescription
            Version = version_razor4_nuget
            ReleaseNotes = toLines release.Notes
            Tags = tags
            OutputPath = outDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies = [ "Microsoft.AspNet.Razor", "4.0.0-beta1" ] })
        "nuget/RazorEngine-razor4.nuspec"

    // Roslyn
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = projectName_roslyn
            Summary = projectSummary_roslyn
            Description = projectDescription_roslyn
            Version = version_roslyn_nuget
            ReleaseNotes = toLines release.Notes
            Tags = tags
            OutputPath = outDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies =
              [ projectName, version_nuget
                "Microsoft.AspNet.Razor", "3.0.0"
                "Microsoft.CodeAnalysis", "1.0.0-beta1-20141031-01" ] })
        "nuget/RazorEngine.Roslyn.nuspec"
    NuGet (fun p -> 
        { p with   
            Authors = authors
            Project = projectName_roslyn
            Summary = projectSummary_roslyn
            Description = projectDescription_roslyn
            Version = version_roslyn_razor4_nuget
            ReleaseNotes = toLines release.Notes
            Tags = tags
            OutputPath = outDir
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey"
            Dependencies =
              [ projectName, version_razor4_nuget
                "Microsoft.AspNet.Razor", "4.0.0-beta1"
                "Microsoft.CodeAnalysis", "1.0.0-beta1-20141031-01" ] })
        "nuget/RazorEngine.Roslyn-razor4.nuspec"

)

// Documentation 

MyTarget "GithubDoc" (fun _ -> buildDocumentationTarget "GithubDoc")

MyTarget "LocalDoc" (fun _ -> 
    buildDocumentationTarget "LocalDoc"
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (outDocDir @@ "local" @@ "html" @@ "index.html")))
)


MyTarget "ReleaseGithubDoc" (fun isSingle ->
    let repro = (sprintf "git@github.com:%s/%s.git" github_user github_project)  
    let doAction =
        if isSingle then true
        else
            printf "update github docs to %s? (y,n): " repro
            let line = System.Console.ReadLine()
            line = "y"
    if doAction then
        CleanDir "gh-pages"
        cloneSingleBranch "" repro "gh-pages" "gh-pages"
        fullclean "gh-pages"
        CopyRecursive ("release"@@"documentation"@@(sprintf "%s.github.io" github_user)@@"html") "gh-pages" true |> printfn "%A"
        StageAll "gh-pages"
        Commit "gh-pages" (sprintf "Update generated documentation %s" release.NugetVersion)
        printf "gh-pages branch updated in the gh-pages directory, push that branch to %s now? (y,n): " repro
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
        
            printf "create tags? (y,n): "
            let line = System.Console.ReadLine()
            if line = "y" then
                let roslyn_tag = (sprintf "roslyn-%s" version_roslyn_nuget)
                let roslyn_razor4_tag = (sprintf "roslyn-%s" version_roslyn_razor4_nuget)
                let doSafe msg f =
                    try
                        f()
                    with exn -> 
                        trace (sprintf "Error (%s): %A" msg exn)

                doSafe "delete_tag version_nuget" 
                    (fun () -> Branches.deleteTag "" version_nuget)
                doSafe "delete_tag version_razor4_nuget" 
                    (fun () -> Branches.deleteTag "" version_razor4_nuget)
                doSafe "delete_tag roslyn_tag" 
                    (fun () -> Branches.deleteTag "" roslyn_tag)
                doSafe "delete_tag roslyn_razor4_tag" 
                    (fun () -> Branches.deleteTag "" roslyn_razor4_tag)
                
                doSafe "create_tag version_nuget" 
                    (fun () -> Branches.tag "" version_nuget)
                doSafe "create_tag version_razor4_nuget" 
                    (fun () -> Branches.tag "" version_razor4_nuget)
                doSafe "create_tag roslyn_tag" 
                    (fun () -> Branches.tag "" roslyn_tag)
                doSafe "create_tag roslyn_razor4_tag" 
                    (fun () -> Branches.tag "" roslyn_razor4_tag)

                printf "push tags? (y,n): "
                let line = System.Console.ReadLine()
                if line = "y" then
                    Branches.pushTag "" "origin" version_nuget
                    Branches.pushTag "" "origin" version_razor4_nuget
                    Branches.pushTag "" "origin" roslyn_tag
                    Branches.pushTag "" "origin" roslyn_razor4_tag

            printf "push branch? (y,n): "
            let line = System.Console.ReadLine()
            if line = "y" then
                Branches.push ""
)

Target "Release" (fun _ ->
    trace "All released!"
)

// Clean all
"Clean" 
  ==> "CleanAll"
"Clean_single" 
  ==> "CleanAll_single"

"Clean"
  ==> "RestorePackages"
  ==> "SetVersions_Razor4" 
  ==> "SetVersions" 
  
"SetVersions_Razor4"
  ==> "BuildApp_Razor4"
"SetVersions"
  ==> "BuildApp_40"
"SetVersions"
  ==> "BuildApp_45"
  
"BuildApp_Razor4"
  ==> "BuildTest_Razor4"
  ==> "Test_Razor4"

"BuildApp_40"
  ==> "BuildTest_40"
  ==> "Test_40"
  
"BuildApp_45"
  ==> "BuildTest_45"
  ==> "Test_45"
  
  
"Test_Razor4"
  ==> "All"
"Test_40"
  ==> "All"
"Test_45"
  ==> "All"


// Dependencies
"Clean" 
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
