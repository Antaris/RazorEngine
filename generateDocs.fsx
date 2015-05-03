// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
open System.IO
// Force to load our build!
// As long as FSharp.Formatting is using the regular net45 build
// This should work as expected.
#I @"build/net45"
#r @"build/net45/System.Web.Razor.dll"
#r @"build/net45/RazorEngine.dll"

#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#load @"buildConfig.fsx"
#load "packages/Yaaf.AdvancedBuilding/content/generateDocsInclude.fsx"
open Fake
// Force load of System.Web.Razor.dll
let someType = typeof<System.Web.Razor.RazorEngineHost>
let printAssemblies msg =
  printfn "%s. Loaded Assemblies:" msg
  System.AppDomain.CurrentDomain.GetAssemblies()
    |> Seq.choose (fun a -> try Some (a.GetName().FullName, a.Location) with _ -> None)
  //|> Seq.filter (fun l -> l.Contains ("Razor"))
    |> Seq.iter (fun (n, l) -> printfn "\t- %s: %s" n l)
  
printAssemblies "starting documentation generation"
try RunTargetOrDefault "LocalDoc"
finally
  printAssemblies "Documentation generation finished"

printfn "THE END"