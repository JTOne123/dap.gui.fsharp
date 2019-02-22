[<AutoOpen>]
module Demo.Gui.Helper

open FSharp.Control.Tasks.V2

open Dap.Prelude
open Dap.Context
open Dap.Platform
open Dap.Gui

open Demo.App
type HomePanel = Demo.Gui.Presenter.HomePanel.Presenter

type App with
    static member RunGui (logFile, ?scope : string, ?consoleMinLevel : LogLevel) : int =
        YogaStyles.register ()
        App.Create (logFile, ?scope = scope, ?consoleMinLevel = consoleMinLevel)
        :> IApp
        |> runGuiApp HomePanel.Create