// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the generation of the docs (it is called by the build automatically). 
*)

#I @".nuget/Build/FAKE/tools/" // FAKE
#I @"lib/FSharp.Formatting" // Custom build with the FSharp.Compiler.Service from below
#I @"build/net40" // dependency
#I @"lib/FSharp.Compiler.Service" // because we want C# support, nuget version doesnt have it (https://github.com/matthid/FSharp.Compiler.Service/commit/c5dfd4dd488f6dcd1024b0ed2b564ce9d2d414fa?diff=unified)
#r @"FakeLib.dll"  //FAKE

#load @"buildConfig.fsx"
open BuildConfig

// Documentation
#r "FSharp.Compiler.Service.dll"
#r "System.Web.Razor.dll"
#r "RazorEngine.dll"
#r "FSharp.Literate.dll"
#r "FSharp.CodeFormat.dll"
#r "FSharp.MetadataFormat.dll"

open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

open FSharp.Literate
open FSharp.MetadataFormat

open RazorEngine.Compilation

// Documentation

/// Processes an entire directory containing multiple script files 
/// (*.fsx) and Markdown documents (*.md) and it specifies additional 
/// replacements for the template file

//let website_root = "file://" + Path.GetFullPath (outDocDir @@ "html")
let buildAllDocumentation outDocDir website_root =
    let references =
        if isMono then
            // Workaround compiler errors in Razor-ViewEngine
            let d = RazorEngine.Compilation.Resolver.UseCurrentAssembliesReferenceResolver()
            let loadedList = d.GetReferences () |> Seq.cache
            //// We replace the list and add required items manually as mcs doesn't like duplicates...
            let getItem name =
                loadedList |> Seq.find (fun l -> l.Contains name)
            [ (getItem "FSharp.Core").Replace("4.3.0.0", "4.3.1.0")  // (if isMono then "/usr/lib64/mono/gac/FSharp.Core/4.3.1.0__b03f5f7f11d50a3a/FSharp.Core.dll" else "FSharp.Core") 
              Path.GetFullPath "./lib/FSharp.Compiler.Service/FSharp.Compiler.Service.dll"
              Path.GetFullPath "./build/net40/System.Web.Razor.dll"
              Path.GetFullPath "./build/net40/RazorEngine.dll"
              Path.GetFullPath "./lib/FSharp.Formatting/FSharp.Literate.dll"
              Path.GetFullPath "./lib/FSharp.Formatting/FSharp.CodeFormat.dll"
              Path.GetFullPath "./lib/FSharp.Formatting/FSharp.MetadataFormat.dll" ] 
            |> Some
        else None
    
    let projInfo =
        [ "root", website_root
          "page-description", "RazorEngine implementation"
          "page-author", "Matthias Dittrich"
          "github-link", github_url
          "project-name", "RazorEngine"
          "project-summary", projectSummary 
          "project-commit", commitHash
          "project-author", authors |> Seq.head
          "project-github", github_url
          "project-issues", sprintf "%s/issues" github_url
          "project-new-issue", sprintf "%s/issues/new" github_url
          "project-nuget", "https://www.nuget.org/packages/RazorEngine/"]

      
    // Copy static files and CSS + JS from F# Formatting
    let copyDocContentFiles () =
      CopyRecursive "./doc/content" (outDocDir @@ "html" @@ "content") true |> Log "Copying file: "
      //ensureDirectory (outDocDir @@ "html" @@ "content")
      //CopyRecursive (formatting @@ "styles") (output @@ "content") true 
      //  |> Log "Copying styles and scripts: "

    let processDirectory(outputKind) =
      let template, outDirName, indexName = 
        match outputKind with
        | OutputKind.Html -> docTemplatesDir @@ "docpage.cshtml", "html", "index.html"
        | OutputKind.Latex -> docTemplatesDir @@ "template-color.tex", "latex", "Readme.tex"
      let outDir = outDocDir @@ outDirName
      Literate.ProcessDirectory
        ( "./doc", template, outDir, 
          outputKind, replacements = projInfo, layoutRoots = layoutRoots, generateAnchors = true, ?assemblyReferences = references)

      Literate.ProcessMarkdown("./readme.md", template, outDir @@ indexName, outputKind, replacements = projInfo, layoutRoots = layoutRoots, generateAnchors = true, ?assemblyReferences = references)
  

    // Build API reference from XML comments
    let referenceBinaries =
        [
            "RazorEngine.dll"
        ]

    let buildReference () =
        let referenceDir = outDocDir @@ "html"
        let outDir = referenceDir @@ "references"
        ensureDirectory referenceDir
        ensureDirectory outDir
        let binaries =
            referenceBinaries
            |> List.map (fun lib-> Path.GetFullPath( buildDir @@ "net45" @@ lib ))
        MetadataFormat.Generate
           (binaries, Path.GetFullPath outDir, layoutRoots,
            parameters = projInfo,
            libDirs = [ Path.GetFullPath ("./build/net45") ],
            otherFlags = [ "-r:System";"-r:System.Core";"-r:System.Xml";"-r:System.Xml.Linq"],
            sourceRepo = github_url + "/blob/master/",
            sourceFolder = "./",
            publicOnly = true, 
            ?assemblyReferences = references )

    CleanDirs [ outDocDir ]
    copyDocContentFiles()
    processDirectory OutputKind.Html
    processDirectory OutputKind.Latex
    buildReference()
    
MyTarget "GithubDoc" (fun _ -> buildAllDocumentation (outDocDir @@ sprintf "%s.github.io" github_user) (sprintf "https://%s.github.io/%s" github_user github_project))

MyTarget "LocalDoc" (fun _ -> 
    buildAllDocumentation (outDocDir @@ "local") ("file://" + Path.GetFullPath (outDocDir @@ "local" @@ "html"))
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (outDocDir @@ "local" @@ "html" @@ "index.html")))
)

RunTargetOrDefault "LocalDoc"