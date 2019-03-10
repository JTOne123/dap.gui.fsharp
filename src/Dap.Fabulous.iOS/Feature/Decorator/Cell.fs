[<RequireQualifiedAccess>]
module Dap.Fabulous.iOS.Feature.Decorator.Cell

open System
open System.Reflection

open Foundation
open CoreGraphics
open UIKit

open Xamarin.Forms
open Xamarin.Forms.Platform.iOS

open Dap.Prelude
open Dap.Context
open Dap.Platform

open Dap.Fabulous.Decorator
open Xamarin.Forms

// Xamarin.Forms.Platform.iOS/Cells/CellRenderer.cs

let getRealCell (logger : ILogger) (cell : Cell) =
    Util.getBindableValue<CellRenderer, UITableViewCell> logger "RealCellProperty" cell
    |> Option.bind (fun cell ->
        if cell <> null then
            Some cell
        else
            logError logger "Cell.getRealCell" "RealCell_Is_Null" (cell)
            None
    )
