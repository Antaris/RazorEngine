// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the generation of the docs (it is called by the build automatically). 
*)

open System.IO
// Force to load our build!
try
  File.Delete ".nuget/Build/FSharp.Formatting/lib/net40/RazorEngine.dll"
with e -> printfn "Could not delete RazorEngine.dll: %s" e.Message
try
  File.Delete ".nuget/Build/FSharp.Formatting/lib/net40/System.Web.Razor.dll"
with e -> printfn "Could not delete System.Web.Razor.dll: %s" e.Message

#I @"build/net40"
#I @".nuget/Build/FSharp.Compiler.Service/lib/net40"
#I @".nuget/Build/FSharp.Formatting/lib/net40"
#I @".nuget/Build/FAKE/tools"
#r @"FakeLib.dll"
#r "System.Web.dll"

open System.Collections.Generic
open System.Web

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

#load @"buildConfig.fsx"
open BuildConfig

// Documentation
#r "FSharp.Compiler.Service.dll"
#r "System.Web.Razor.dll"
#r "RazorEngine.dll"

#r "FSharp.Markdown.dll"
#r "FSharp.Literate.dll"
#r "FSharp.CodeFormat.dll"
#r "FSharp.MetadataFormat.dll"

open FSharp.Markdown
open FSharp.Literate
open FSharp.MetadataFormat

open RazorEngine.Compilation

// Documentation

/// Processes an entire directory containing multiple script files 
/// (*.fsx) and Markdown documents (*.md) and it specifies additional 
/// replacements for the template file

//let website_root = "file://" + Path.GetFullPath (outDocDir @@ "html")
let formattingContext templateFile format generateAnchors replacements layoutRoots =
    { TemplateFile = templateFile 
      Replacements = defaultArg replacements []
      GenerateLineNumbers = true
      IncludeSource = false
      Prefix = "fs"
      OutputKind = defaultArg format OutputKind.Html
      GenerateHeaderAnchors = defaultArg generateAnchors false
      LayoutRoots = defaultArg layoutRoots [] }

let rec replaceCodeBlocks ctx = function
    | Matching.LiterateParagraph(special) -> 
        match special with
        | LanguageTaggedCode(lang, code) -> 
            let inlined = 
              match ctx.OutputKind with
              | OutputKind.Html ->
                  let code = HttpUtility.HtmlEncode code
                  let codeHtmlKey = sprintf "language-%s" lang
                  sprintf "<pre class=\"line-numbers %s\"><code class=\"%s\">%s</code></pre>" codeHtmlKey codeHtmlKey code
              | OutputKind.Latex ->
                  sprintf "\\begin{lstlisting}\n%s\n\\end{lstlisting}" code
            Some(InlineBlock(inlined))
        | _ -> Some (EmbedParagraphs special)
    | Matching.ParagraphNested(pn, nested) ->
        let nested = List.map (List.choose (replaceCodeBlocks ctx)) nested
        Some(Matching.ParagraphNested(pn, nested))
    | par -> Some par

let buildAllDocumentation outDocDir website_root =
    let references =
        if isMono then
            // Workaround compiler errors in Razor-ViewEngine
            let d = RazorEngine.Compilation.ReferenceResolver.UseCurrentAssembliesReferenceResolver()
            let loadedList = d.GetReferences() |> Seq.map (fun c -> c.GetFile()) |> Seq.cache
            //// We replace the list and add required items manually as mcs doesn't like duplicates...
            let getItem name =
                loadedList |> Seq.find (fun l -> l.Contains name)
            [ (getItem "FSharp.Core").Replace("4.3.0.0", "4.3.1.0")  // (if isMono then "/usr/lib64/mono/gac/FSharp.Core/4.3.1.0__b03f5f7f11d50a3a/FSharp.Core.dll" else "FSharp.Core") 
              Path.GetFullPath "./.nuget/Build/FSharp.Compiler.Service/lib/net40/FSharp.Compiler.Service.dll"
              Path.GetFullPath "./build/net40/System.Web.Razor.dll"
              Path.GetFullPath "./build/net40/RazorEngine.dll"
              Path.GetFullPath "./.nuget/Build/FSharp.Formatting/lib/net40/FSharp.Literate.dll"
              Path.GetFullPath "./.nuget/Build/FSharp.Formatting/lib/net40/FSharp.CodeFormat.dll"
              Path.GetFullPath "./.nuget/Build/FSharp.Formatting/lib/net40/FSharp.MetadataFormat.dll" ]
            |> Some
        else None

    let projInfo =
        [ "root", website_root
          "page-description", projectDescription
          "page-author", page_author
          "github-link", github_url
          "project-name", projectName
          "project-summary", projectSummary 
          "project-commit", commitHash
          "project-author", authors |> Seq.head
          "project-github", github_url
          "project-issues", sprintf "%s/issues" github_url
          "project-new-issue", sprintf "%s/issues/new" github_url
          "project-nuget", nuget_url]

    // Copy static files and CSS + JS from F# Formatting
    let copyDocContentFiles () =
      ensureDirectory (outDocDir @@ "html" @@ "content")
      CopyRecursive "./doc/content" (outDocDir @@ "html" @@ "content") true |> Log "Copying file: "

    let processDocumentationFiles(outputKind) =
      let indexTemplate, template, outDirName, indexName = 
        match outputKind with
        | OutputKind.Html -> docTemplatesDir @@ "docpage-index.cshtml", docTemplatesDir @@ "docpage.cshtml", "html", "index.html"
        | OutputKind.Latex -> docTemplatesDir @@ "template-color.tex", docTemplatesDir @@ "template-color.tex", "latex", "Readme.tex"
      let outDir = outDocDir @@ outDirName
      let handleDoc template (doc:LiterateDocument) outfile =
        // prismjs support
        let ctx = formattingContext (Some template) (Some outputKind) (Some true) (Some projInfo) (Some layoutRoots)
        let newParagraphs = List.choose (replaceCodeBlocks ctx) doc.Paragraphs
        Templating.processFile references (doc.With(paragraphs = newParagraphs)) outfile ctx

      let processMarkdown template infile outfile =
        let doc = Literate.ParseMarkdownFile( infile )
        handleDoc template doc outfile
      let processScriptFile template infile outfile =
        let doc = Literate.ParseScriptFile( infile )
        handleDoc template doc outfile

      let rec processDirectory template indir outdir = 
        // Create output directory if it does not exist
        if Directory.Exists(outdir) |> not then
          try Directory.CreateDirectory(outdir) |> ignore 
          with _ -> failwithf "Cannot create directory '%s'" outdir

        let fsx = [ for f in Directory.GetFiles(indir, "*.fsx") -> processScriptFile template, f ]
        let mds = [ for f in Directory.GetFiles(indir, "*.md") -> processMarkdown template, f ]
        for func, file in fsx @ mds do
          let dir = Path.GetDirectoryName(file)
          let name = Path.GetFileNameWithoutExtension(file)
          let ext = (match outputKind with OutputKind.Latex -> "tex" | _ -> "html")
          let output = Path.Combine(outdir, sprintf "%s.%s" name ext)

          // Update only when needed
          let changeTime = File.GetLastWriteTime(file)
          let generateTime = File.GetLastWriteTime(output)
          if changeTime > generateTime then
            printfn "Generating '%s/%s.%s'" dir name ext
            func file output
      
        for d in Directory.EnumerateDirectories(indir) do
          let name = Path.GetFileName(d)
          processDirectory template (Path.Combine(indir, name)) (Path.Combine(outdir, name))
          
      processDirectory template "./doc" outDir
      processMarkdown indexTemplate "./README.md" (outDir @@ indexName)
      processMarkdown template "./CONTRIBUTING.md" (outDir @@ "Contributing" + (if outDirName = "html" then ".html" else ".tex"))

    // Build API reference from XML comments
    let referenceBinaries =
        [
            "RazorEngine.dll"
            "RazorEngine.Roslyn.dll"
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
            markDownComments = false, // <see cref=""/> support 
            ?assemblyReferences = references )

    CleanDirs [ outDocDir ]
    copyDocContentFiles()
    processDocumentationFiles OutputKind.Html
    processDocumentationFiles OutputKind.Latex
    buildReference()
    
MyTarget "GithubDoc" (fun _ -> buildAllDocumentation (outDocDir @@ sprintf "%s.github.io" github_user) (sprintf "https://%s.github.io/%s" github_user github_project))

MyTarget "LocalDoc" (fun _ -> 
    buildAllDocumentation (outDocDir @@ "local") ("file://" + Path.GetFullPath (outDocDir @@ "local" @@ "html"))
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (outDocDir @@ "local" @@ "html" @@ "index.html")))
)

RunTargetOrDefault "LocalDoc"