[<AutoOpen>]
module Dap.Forms.Prefab.TextField

//SILP: COMMON_OPENS
open System                                                           //__SILP__
open Xamarin.Forms                                                    //__SILP__
open Dap.Prelude                                                      //__SILP__
open Dap.Context                                                      //__SILP__
open Dap.Platform                                                     //__SILP__
open Dap.Gui                                                          //__SILP__
open Dap.Gui.Prefab                                                   //__SILP__
open Dap.Gui.Internal                                                 //__SILP__

type TextFieldWidget = Xamarin.Forms.Entry

//SILP: PREFAB_HEADER_MIDDLE(TextField)
type TextField (logging : ILogging) =                                            //__SILP__
    inherit BasePrefab<TextField, TextFieldProps, TextFieldWidget>               //__SILP__
        (TextFieldKind, TextFieldProps.Create, logging, new TextFieldWidget ())  //__SILP__
    do (                                                                         //__SILP__
        let kind = TextFieldKind                                                 //__SILP__
        let owner = base.AsOwner                                                 //__SILP__
        let model = base.Model                                                   //__SILP__
        let widget = base.Widget                                                 //__SILP__
        model.Text.OnChanged.AddWatcher owner kind (fun evt ->
            widget.Text <- evt.New
        )
        widget.TextChanged.Add (fun _ ->
            model.Text.SetValue widget.Text
        )
    )
    //SILP: PREFAB_FOOTER(TextField)
    static member Create l = new TextField (l)                        //__SILP__
    static member Create () = new TextField (getLogging ())           //__SILP__
    override this.Self = this                                         //__SILP__
    override __.Spawn l = TextField.Create l                          //__SILP__
    interface IFallback                                               //__SILP__
    interface ITextField