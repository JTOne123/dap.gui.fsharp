(* FAKE: 5.13.3 *)
#r "paket: groupref Build //"
#load ".fake/build.fsx/intellisense.fsx"

#load "../src/Dap.Gui/Dsl/Models.fs"
#load "../src/Dap.Gui/Dsl/PaletteParam.fs"

#load "../src/Dap.Gui/_Gen/Models.fs"
#load "../src/Dap.Gui/_Gen/Builder/Models.fs"
#load "../src/Dap.Gui/_Gen/Builder/Internal/Base.fs"
#load "../src/Dap.Gui/ContainerKind.fs"
#load "../src/Dap.Gui/Builder/Helper.fs"
#load "../src/Dap.Gui/Generator/Types.fs"
#load "../src/Dap.Gui/Generator/Prefab.fs"
#load "../src/Dap.Gui/Generator/Helper.fs"
#load "../src/Dap.Gui/Dsl1/Prefabs.fs"

#load "Demo.App/Dsl.fs"
#load "Demo.Gui/Dsl/Prefabs.fs"

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators

open Dap.Build

open FSharp.Data
open Dap.Prelude
open Dap.Context.Generator.Util

[<Literal>]
let Prepare = "Prepare"

let projects =
    !! "Demo.App/*.fsproj"
    ++ "Demo.Gui/*.fsproj"
    ++ "Demo.Myra/*.fsproj"
    ++ "Demo.Gtk/*.fsproj"

DotNet.create DotNet.debug projects

DotNet.createPrepares [
    ["Demo.App"], fun _ ->
        Demo.App.Dsl.compile ["Demo.App"]
        |> List.iter traceSuccess
    ["Demo.Gui"], fun _ ->
        Demo.Gui.Dsl.Prefabs.compile ["Demo.Gui"]
        |> List.iter traceSuccess
]

Target.runOrDefault DotNet.Build
