// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#load @"buildConfig.fsx"
#load "packages/Yaaf.AdvancedBuilding/content/buildInclude.fsx"

open System.IO
open Fake
let config = BuildInclude.config
// Define your FAKE targets here

let MyTarget = BuildInclude.MyTarget

BuildInclude.documentationFAKEArgs <- "-nc"

// This step ensures our current build is still compatible with FSharp.Formatting.
MyTarget "CopyToFSharpFormatting" (fun _ ->
    // make the FSF load script happy
    [ "build/net45/RazorEngine.dll"; "packages/net45/Microsoft.AspNet.Razor/lib/net45/System.Web.Razor.dll" ]
    |> Seq.iter (fun source ->
      let dest = sprintf "packages/FSharp.Formatting/lib/net40/%s" (Path.GetFileName source)
      //try
      if File.Exists dest then
        trace (sprintf "Deleting %s" dest)
        File.Delete dest
      trace (sprintf "Copying %s to %s" source dest)
      File.Copy (source, dest)
      //with e ->
      //  trace (sprintf "Couldn't copy %s to %s, because: %O" source dest e)
    ))

"AfterBuild"
    ==> "CopyToFSharpFormatting"
    ==> "AllDocs"

// start build
RunTargetOrDefault "All"
