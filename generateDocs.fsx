// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
open System.IO
// Force to load our build!
// As long as FSharp.Formatting is using the regular net45 build
// This should work as expected.
#I @"build/net45"
#r "System.Web.Razor.dll"
#r "RazorEngine.dll"

#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#load @"buildConfig.fsx"
#load "packages/Yaaf.AdvancedBuilding/content/generateDocsInclude.fsx"
open Fake
try
  RunTargetOrDefault "LocalDoc"
with e ->
  printfn "Exception in documentation generation. Loaded Assemblies:"
  System.AppDomain.CurrentDomain.GetAssemblies()
  |> Seq.choose (fun a -> try Some (a.GetName().FullName, a.Location) with _ -> None)
  //|> Seq.filter (fun l -> l.Contains ("Razor"))
  |> Seq.iter (fun (n, l) -> printfn "\t- %s: %s" n l)
  reraise()