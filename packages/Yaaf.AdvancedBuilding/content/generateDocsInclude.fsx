// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the generation of the docs (it is called by the build automatically). 
*)
#if FAKE
#else
// Support when file is opened in Visual Studio
#load "buildConfigDef.fsx"
#load "../../../buildConfig.fsx"
#endif

open BuildConfigDef
let config = BuildConfig.buildConfig.FillDefaults()

#load @"../../FSharp.Formatting/FSharp.Formatting.fsx"
#r "System.Web"

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

let editLiterateDocument ctx (doc:LiterateDocument) =
  doc.With(paragraphs = List.choose (replaceCodeBlocks ctx) doc.Paragraphs)

let printAssemblies msg =
  printfn "%s. Loaded Assemblies:" msg
  System.AppDomain.CurrentDomain.GetAssemblies()
    |> Seq.choose (fun a -> try Some (a.GetName().FullName, a.Location) with _ -> None)
  //|> Seq.filter (fun l -> l.Contains ("Razor"))
    |> Seq.iter (fun (n, l) -> printfn "\t- %s: %s" n l)

// ITS VERY IMPORTANT TO CREATE THE EVALUATOR LAZY (see https://github.com/matthid/Yaaf.AdvancedBuilding/issues/5)
let evalutator = lazy (Some <| (FsiEvaluator() :> IFsiEvaluator))
//let evalutator = lazy None

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
        | OutputKind.Html -> "docpage-index.cshtml", "docpage.cshtml", "html", "index.html", ".html"
        | OutputKind.Latex -> 
          config.DocTemplatesDir @@ "template-color.tex", config.DocTemplatesDir @@ "template-color.tex", 
          "latex", "Readme.tex", ".tex"
      let outDir = outDocDir @@ outDirName
      let handleDoc template (doc:LiterateDocument) outfile =
        // prismjs support
        let ctx = formattingContext (Some template) (Some outputKind) (Some true) (Some projInfo) (Some config.LayoutRoots)
        Templating.processFile references (editLiterateDocument ctx doc) outfile ctx 

      let processMarkdown template infile outfile =
        let doc = Literate.ParseMarkdownFile( infile, ?fsiEvaluator = evalutator.Value )
        handleDoc template doc outfile
      let processScriptFile template infile outfile =
        let doc = Literate.ParseScriptFile( infile, ?fsiEvaluator = evalutator.Value )
        handleDoc template doc outfile
        
      let rec processDirectory template indir outdir = 
        Literate.ProcessDirectory(
          indir, template, outdir, outputKind, generateAnchors = true, replacements = projInfo, 
          layoutRoots = config.LayoutRoots, customizeDocument = editLiterateDocument,
          processRecursive = true, includeSource = true, ?fsiEvaluator = evalutator.Value,
          ?assemblyReferences = references)

      processDirectory template (Path.GetFullPath "./doc") outDir
      let processFile template inFile outFile =
        if File.Exists inFile then
          processMarkdown template inFile outFile
        else
          trace (sprintf "File %s was not found so %s was not created!" inFile outFile)
      
      // Handle some special files.
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
            |> List.map (fun lib -> libDir @@ lib)

        MetadataFormat.Generate
           (binaries, Path.GetFullPath outDir, config.LayoutRoots,
            parameters = projInfo,
            libDirs = [ libDir ],
            otherFlags = [],
            sourceRepo = config.SourceReproUrl,
            sourceFolder = "./",
            publicOnly = true, 
            markDownComments = false, // <see cref=""/> support 
            ?assemblyReferences = references )

    CleanDirs [ outDocDir ]
    copyDocContentFiles()

    try
      // FIRST build the reference documentation, see https://github.com/matthid/Yaaf.AdvancedBuilding/issues/5
      buildReference()
      processDocumentationFiles OutputKind.Html
      processDocumentationFiles OutputKind.Latex
    with e ->
      printAssemblies "(DIAGNOSTICS) Documentation failed"
      reraise()
    
let MyTarget name body =
    Target name (fun _ -> body false)
    let single = (sprintf "%s_single" name)
    Target single (fun _ -> body true)

let doGithub () =
    buildAllDocumentation (config.OutDocDir @@ sprintf "%s.github.io" config.GithubUser) (sprintf "https://%s.github.io/%s" config.GithubUser config.GithubProject)

let doLocal () =
    buildAllDocumentation (config.OutDocDir @@ "local") ("file://" + Path.GetFullPath (config.OutDocDir @@ "local" @@ "html"))
    trace (sprintf "Local documentation has been finished, you can view it by opening %s in your browser!" (Path.GetFullPath (config.OutDocDir @@ "local" @@ "html" @@ "index.html")))

let watch () =
  printfn "Starting watching by initial building..."
  let rebuildDocs () =
    CleanDir (config.OutDocDir @@ "local") // Just in case the template changed (buildDocumentation is caching internally, maybe we should remove that)
    doLocal()
  rebuildDocs()
  printfn "Watching for changes..."

  let full s = Path.GetFullPath s
  let queue = new System.Collections.Concurrent.ConcurrentQueue<_>()
  let processTask () =
    async {
      let! tok = Async.CancellationToken
      while not tok.IsCancellationRequested do
        try
          if queue.IsEmpty then
            do! Async.Sleep 1000
          else
            let data = ref []
            let hasData = ref true
            while !hasData do
              match queue.TryDequeue() with
              | true, d ->
                data := d :: !data
              | _ ->
                hasData := false

            printfn "Detected changes (%A). Invalidate cache and rebuild." !data
            //global.FSharp.MetadataFormat.RazorEngineCache.InvalidateCache (!data |> Seq.map (fun change -> change.FullPath))
            //global.FSharp.Literate.RazorEngineCache.InvalidateCache (!data |> Seq.map (fun change -> change.FullPath))
            rebuildDocs()
            printfn "Documentation generation finished."
        with e ->
          printfn "Documentation generation failed: %O" e
    }
  use watcher =
    !! (full "." + "/**/*.*")
    |> WatchChanges (fun changes ->
      changes
      |> Seq.filter (fun change ->
        change.Name.StartsWith("doc" @@ "") || 
        change.Name = "README.md" ||
        change.Name = "CONTRIBUTING.md" ||
        change.Name = "LICENSE.md")
      |> Seq.iter queue.Enqueue
    )
  use source = new System.Threading.CancellationTokenSource()
  Async.Start(processTask (), source.Token)
  printfn "Press enter to exit watching..."
  System.Console.ReadLine() |> ignore
  watcher.Dispose()
  source.Cancel()

MyTarget "GithubDoc" (fun _ -> doGithub())

MyTarget "LocalDoc" (fun _ -> doLocal())

MyTarget "WatchDocs" (fun _ -> watch())

MyTarget "AllDocs" (fun _ ->
    if config.EnableGithub then doGithub()
    doLocal()
)
