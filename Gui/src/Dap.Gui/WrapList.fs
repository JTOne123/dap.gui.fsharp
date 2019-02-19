[<AutoOpen>]
module Dap.Gui.WrapList

open Dap.Prelude
open Dap.Context
open Dap.Platform

[<AbstractClass>]
type WrapList<'prefab, 'model, 'item_prefab, 'item_model, 'layout
        when 'prefab :> IPrefab
            and 'model :> ListProps<'item_model>
            and 'item_prefab :> IPrefab<'item_model>
            and 'item_model :> IViewProps
            and 'layout :> IListLayout
            and 'layout :> IFeature> (kind : Kind, spawner, logging : ILogging) =
    inherit CustomContext<'prefab, ContextSpec<'model>, 'model> (logging, new ContextSpec<'model> (kind, spawner))
    let target = Feature.create<'layout> logging
    let mutable batching : bool = false
    let setTargetItems (items : 'item_model list) =
        target.SetItems items Feature.create<'item_prefab>
    do (
        let owner = base.AsOwner
        let items = base.Properties.Items
        items.OnAdded.AddWatcher owner kind (fun evt ->
            if not batching then setTargetItems items.Value
        )
        items.OnRemoved.AddWatcher owner kind (fun evt ->
            if not batching then setTargetItems items.Value
        )
        items.OnMoved.AddWatcher owner kind (fun evt ->
            if not batching then setTargetItems items.Value
        )
    )
    member __.ResizeItems (size : int) =
        let items = base.Properties.Items
        if size = items.Count then
            ()
        else
            batching <- true
            if size < items.Count then
                while size < items.Count do
                    items.Remove (items.Count - 1) |> ignore
            elif size > items.Count then
                while size > items.Count do
                    items.Add () |> ignore
            setTargetItems items.Value
            batching <- false
    member __.Target = target
    member this.Model = this.Properties
    member this.LoadJson' (json : Json) =
        this.Model.AsProperty.LoadJson json
        target.SetWrapper' this json
    interface IPrefab with
        member __.Wrapper = None
        member __.Widget0 = target.Widget1
        member __.Styles = target.Styles
        member __.ApplyStyles () = target.ApplyStyles ()
        member this.SetWrapper' w j = failWith "Already_Is_Wrapper" (this, w, j)
    interface ILayout with
        member __.Widget1 = target.Widget1
    interface IListLayout with
        member __.Prefabs0 = target.Prefabs0
        member __.SetItems<'p, 'm when 'p :> IPrefab<'m> and 'm :> IViewProps> (items : 'm list) (spawner : ILogging -> 'p) =
            target.SetItems<'p, 'm> items spawner
    interface IListLayout<'model>
    interface IListLayout<'model, 'item_prefab> with
        member __.Prefabs =
            target.Prefabs0
            |> List.map (fun item -> item :?> 'item_prefab)
    member this.AsLayout = this :> ILayout

    member this.AsListLayout = this :> IListLayout
    member this.AsPrefab = this.Self
