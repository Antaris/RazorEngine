// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the generation of the docs (it is called by the build automatically). 
*)

open BuildConfigDef
let config = BuildConfig.buildConfig.FillDefaults()

#load @"../../FSharp.Formatting/FSharp.Formatting.fsx"

open System.Collections.Generic
open System.IO
open System.Web

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

open FSharp.Markdown
open FSharp.Literate
open FSharp.MetadataFormat

open RazorEngine.Compilation

let commitHash = lazy Information.getCurrentSHA1(".")

// Documentation

/// Processes an entire directory containing multiple script files 
/// (*.fsx) and Markdown documents (*.md) and it specifies additional 
/// replacements for the template file

//let website_root = "file://" + Path.GetFullPath (config.OutDocDir @@ "html")

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
    let references = config.DocRazorReferences
    
    let projInfo =
        [ "root", website_root
          "page-description", config.ProjectDescription
          "page-author", config.PageAuthor
          "github-link", config.GithubUrl
          "project-name", config.ProjectName
          "project-summary", config.ProjectSummary
          "project-commit", commitHash.Value
          "project-author", config.ProjectAuthors |> Seq.head
          "project-github", config.GithubUrl
          "project-issues", config.IssuesUrl
          "project-new-issue", config.FileNewIssueUrl
          "project-nuget", config.NugetUrl]

      
    // Copy static files and CSS + JS from F# Formatting
    let copyDocContentFiles () =
      ensureDirectory (outDocDir @@ "html" @@ "content")
      CopyRecursive "./doc/content" (outDocDir @@ "html" @@ "content") true |> Log "Copying file: "
      //CopyRecursive (formatting @@ "styles") (output @@ "content") true 
      //  |> Log "Copying styles and scripts: "

      
    let processDocumentationFiles(outputKind) =
      let indexTemplate, template, outDirName, indexName, extension =
        match outputKind with
        | OutputKind.Html -> config.DocTemplatesDir @@ "docpage-index.cshtml", config.DocTemplatesDir @@ "docpage.cshtml", "html", "index.html", ".html"
        | OutputKind.Latex -> config.DocTemplatesDir @@ "template-color.tex", config.DocTemplatesDir @@ "template-color.tex", "latex", "Readme.tex", ".tex"
      let outDir = outDocDir @@ outDirName
      let handleDoc template (doc:LiterateDocument) outfile =
        // prismjs support
        let ctx = formattingContext (Some template) (Some outputKind) (Some true) (Some projInfo) (Some config.LayoutRoots)
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
      let processFile template inFile outFile =
        if File.Exists inFile then
          processMarkdown template inFile outFile
        else
          trace (sprintf "File %s was not found so %s was not created!" inFile outFile)

      processFile indexTemplate "./README.md" (outDir @@ indexName)
      processFile template "./CONTRIBUTING.md" (outDir @@ "Contributing" + extension)
      processFile template "./LICENSE.md" (outDir @@ "License" + extension)

    // Build API reference from XML comments
    let referenceBinaries =
        let xmlFiles = config.GeneratedFileList |> List.filter (fun f -> f.EndsWith(".xml"))
        config.GeneratedFileList
          |> List.filter (fun f -> f.EndsWith(".dll") || f.EndsWith(".exe"))
          |> List.filter (fun f ->
              let exists =
                xmlFiles |> List.exists (fun xml ->
                    Path.GetFileNameWithoutExtension xml = Path.GetFileNameWithoutExtension f)
              if not exists then
                  trace (sprintf "No .xml file is given in GeneratedFileList for %s" f)
              exists)

    let buildReference () =
        let referenceDir = outDocDir @@ "html"
        let outDir = referenceDir @@ "references"
        ensureDirectory referenceDir
        ensureDirectory outDir
        let libDir = config.BuildDir @@ (config.BuildTargets |> Seq.last).SimpleBuildName
        let binaries =
            referenceBinaries
            |> List.map (fun lib -> Path.GetFullPath( libDir @@ lib ))
        let blacklist = [ "FSharp.Core.dll"; "mscorlib.dll" ]
        let libraries =
            Directory.EnumerateFiles(libDir, "*.dll")
            |> Seq.map Path.GetFullPath
            |> Seq.filter (fun file -> binaries |> List.exists (fun binary -> binary = file) |> not)
            |> Seq.append [ "System";"System.Core";"System.Xml";"System.Xml.Linq" ]
            |> Seq.filter (fun file ->
                let name = Path.GetFileName file
                let isBlacklisted = blacklist |> List.exists (fun b -> b = name)
                if isBlacklisted then
                  trace (sprintf "WARNING: Reference to \"%s\" is ignored because it is blacklisted!" file)
                not isBlacklisted)
            |> Seq.map (sprintf "-r:%s")
            |> Seq.toList
        MetadataFormat.Generate
           (binaries, Path.GetFullPath outDir, config.LayoutRoots,
            parameters = projInfo,
            libDirs = [ ],
            otherFlags = libraries,
            sourceRepo = config.SourceReproUrl,
            sourceFolder = "./",
            publicOnly = true, 
            markDownComments = false, // <see cref=""/> support 
            ?assemblyReferences = references )

    CleanDirs [ outDocDir ]
    copyDocContentFiles()
    processDocumentationFiles OutputKind.Html
    // enable when working again...
    //processDocumentationFiles OutputKind.Latex
    buildReference()
    
let MyTarget name body =
    Target name (fun _ -> body false)
    let single = (sprintf "%s_single" name)
    Target single (fun _ -> body true)

let doGithub () =
    buildAllDocumentation (config.OutDocDir @@ sprintf "%s.github.io" config.GithubUser) (sprintf "https://%s.github.io/%s" config.GithubUser config.GithubProject)

let doLocal () =
    buildAllDocumentation (config.OutDocDir @@ "local") ("file://" + Path.GetFullPath (config.OutDocDir @@ "local" @@ "html"))
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (config.OutDocDir @@ "local" @@ "html" @@ "index.html")))

MyTarget "GithubDoc" (fun _ -> doGithub())

MyTarget "LocalDoc" (fun _ -> doLocal())

MyTarget "AllDocs" (fun _ ->
    doGithub()
    doLocal()
)
