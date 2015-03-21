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
RunTargetOrDefault "LocalDoc"