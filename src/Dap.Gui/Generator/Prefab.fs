[<AutoOpen>]
module Dap.Gui.Generator.Prefab

open Newtonsoft.Json.Linq

open Dap.Prelude
open Dap.Context
open Dap.Context.Meta
open Dap.Context.Generator
open Dap.Context.Generator.Util
open Dap.Gui

let private getPrefab (meta : IViewProps) =
    (meta.GetType ()) .Name
        .Replace ("Props", "")
    (*
    match meta with
    | :? ComboProps as combo ->
        ComboLayoutKind.ParseToPrefab combo.Layout.Value
    | :? ListProps as list ->
        ListLayoutKind.ParseToPrefab list.Layout.Value
    | _ ->
        (meta.GetType ()) .Name
            .Replace ("Props", "")
    *)

let private getModelClass (meta : IViewProps) =
    match meta with
    | :? ComboProps as _combo ->
        "ComboProps"
    | :? ListProps as _list ->
        "ListProps"
    | _ ->
        (meta.GetType ()) .Name

let private getModelParam (meta : IViewProps) =
    match meta with
    | :? ListProps as _list ->
        ""
    | _ -> ""

let private getParentInterfaces (meta : IViewProps) (param : PrefabParam) =
    [
        match meta with
        | :? ComboProps as combo ->
            yield sprintf "IComboPrefab<%sProps>" param.Name
            yield sprintf "IGroupPrefab<I%s>" combo.Container.Value.AsCodeMemberName
        | :? ListProps as list ->
            yield sprintf "IListPrefab<%sProps, I%s>" param.Name list.ItemPrefab.Value.AsCodeMemberName
            yield sprintf "IGroupPrefab<I%s>" list.Container.Value.AsCodeMemberName
        | _ ->
            yield sprintf "IPrefab<%sProps>" param.Name
    ]

let private getParentClass (meta : IViewProps) (param : PrefabParam) =
    match meta with
    | :? ComboProps as combo ->
        sprintf "BaseCombo<%s, %sProps, I%s>" param.Name param.Name combo.Container.Value.AsCodeMemberName
    | :? ListProps as list ->
        let item = list.ItemPrefab.Value.AsCodeMemberName
        sprintf "BaseList<%s, %sProps, I%s, %sProps, I%s>" param.Name param.Name item item list.Container.Value.AsCodeMemberName
    | _ ->
        "Unsupported_ParentClass"

type Generator (meta : IViewProps) =
    let getModelClass () =
        getModelClass meta
    let getModelParam () =
        getModelParam meta
    let getParentInterfaces (param : PrefabParam) =
        getParentInterfaces meta param
    let getParentClass (param : PrefabParam) =
        getParentClass meta param
    let getChildPrefab (child : IViewProps) =
        if child.Prefab.Value <> "" then
            child.Prefab.Value.AsCodeMemberName
        else
            getPrefab child
    let getKind (param : PrefabParam) =
        [
            sprintf "[<Literal>]"
            sprintf "let %sKind = \"%s\"" param.Name param.Name
        ]
    let getMetaJson () : Json =
        meta.ToJson ()
        |> match meta with
            | :? ListProps as _list ->
                fun json ->
                    let json = json.Value<JObject> ()
                    json.Add ("items", E.emptyList)
                    json :> Json
            | _ -> id

    let getJson (param : PrefabParam) =
        [
            sprintf "let %sJson = parseJson \"\"\"" param.Name
            E.encode 4 <| getMetaJson ()
            sprintf "\"\"\""
        ]
    let getInterfaceMember (child : IViewProps) =
        let key = child.Spec0.Key
        let prefab = getChildPrefab child
        sprintf "    abstract %s : I%s with get" key.AsCodeMemberName prefab
    let getInterface (param : PrefabParam) =
        let modelClass = getModelClass ()
        [
            yield sprintf "type %sProps = %s" param.Name modelClass
            yield sprintf ""
            yield sprintf "type I%s =" param.Name
            for parent in getParentInterfaces param do
                yield sprintf "    inherit %s" parent
            match meta with
            | :? ComboProps as combo ->
                for prop in combo.Children.Value do
                    match prop with
                    | :? IViewProps as prop ->
                        yield getInterfaceMember prop
                    | _ ->
                        ()
            | :? ListProps as _list ->
                ()
            | _ ->
                ()
        ]
    let getClassHeader (param : PrefabParam) =
        let modelParam = getModelParam ()
        let parentClass = getParentClass param
        [
            sprintf "type %s (logging : ILogging) =" param.Name
            sprintf "    inherit %s (%sKind, %sProps.Create%s, logging)" parentClass param.Name param.Name modelParam
        ]
    let getChildAdder (child : IViewProps) =
        let key = child.Spec0.Key
        let prefab = getChildPrefab child
        sprintf "    let %s : I%s = base.AsComboPrefab.Add \"%s\" Feature.create<I%s>" key.AsCodeVariableName prefab key prefab
    let getClassFields (_param : PrefabParam) =
        [
            match meta with
            | :? ComboProps as combo ->
                for prop in combo.Children.Value do
                    match prop with
                    | :? IViewProps as prop ->
                        yield getChildAdder prop
                    | _ ->
                        ()
            | _ ->
                ()
        ]
    let getClassMiddle (param : PrefabParam) =
        [
            yield sprintf "    do ("
            yield sprintf "        base.LoadJson' %sJson" param.Name
            yield sprintf "    )"
            yield sprintf "    static member Create l = new %s (l)" param.Name
            yield sprintf "    static member Create () = new %s (getLogging ())" param.Name
            yield sprintf "    override this.Self = this"
            yield sprintf "    override __.Spawn l = %s.Create l" param.Name
        ]
    let getChildMember (child : IViewProps) =
        let key = child.Spec0.Key
        let prefab = getChildPrefab child
        sprintf "    member __.%s : I%s = %s" key.AsCodeMemberName prefab key.AsCodeVariableName
    let getClassMembers (_param : PrefabParam) =
        [
            match meta with
            | :? ComboProps as combo ->
                for prop in combo.Children.Value do
                    match prop with
                    | :? IViewProps as prop ->
                        yield getChildMember prop
                    | _ ->
                        ()
            | _ ->
                ()
        ]
    let hasChild () =
        match meta with
        | :? ComboProps as combo ->
            combo.Children.Value
            |> List.exists (fun prop ->
                match prop with
                | :? IViewProps -> true
                | _ -> false
            )
        | :? ListProps as _list ->
            true
        | _ ->
            false

    let getClassFooter (param : PrefabParam) =
        [
            yield sprintf "    interface IFallback"
            yield sprintf "    interface I%s%s" param.Name (if hasChild () then " with" else "")
            match meta with
            | :? ComboProps as combo ->
                yield "        member this.Container = this.AsGroupPrefab.Container"
                for prop in combo.Children.Value do
                    match prop with
                    | :? IViewProps as prop ->
                        yield "    " + getChildMember prop
                    | _ ->
                        ()
            | :? ListProps as _list ->
                yield "        member this.Container = this.AsGroupPrefab.Container"
            | _ ->
                ()
        ]
    interface IGenerator<PrefabParam> with
        member this.Generate param =
            [
                ["open Dap.Platform"]
                ["open Dap.Gui"]
                ["open Dap.Gui.Prefab"]
                [""]
                getKind param
                [""]
                getJson param
                [""]
                getInterface param
                [""]
                getClassHeader param
                getClassFields param
                getClassMiddle param
                getClassMembers param
                getClassFooter param
            ]|> List.concat

type BuilderGenerator (meta : IViewProps) =
    let getBuilderHeader (param : PrefabParam) =
        let prefab = meta.Prefab.Value
        [
            yield sprintf "type %s () =" param.Name
            yield sprintf "    inherit ComboPrefabBuilder (\"%s\", %s.Clone (noOwner, NoKey))" prefab.AsCodeJsonKey prefab.AsCodeMemberName
        ]
    let getBuilderFooter (param : PrefabParam) =
        [
            sprintf ""
            sprintf "let %s = new %s ()" meta.Prefab.Value.AsCodeJsonKey param.Name
        ]
    let getComboOperation (_param : PrefabParam) (prop : IViewProps) =
        let memberName = prop.Key.AsCodeMemberName
        let varName = prop.Key.AsCodeVariableName
        let varType = (prop.GetType ()) .Name
        [
            yield sprintf "    [<CustomOperation(\"%s\")>]" prop.Key.AsCodeJsonKey

            yield sprintf "    member __.%s (target : ComboProps, %s : %s) =" memberName varName varType
            yield sprintf "        %s.SyncTo <| target.Children.Get<%s> \"%s\"" varName varType prop.Key
            yield sprintf "        target"
            yield sprintf "    [<CustomOperation(\"update_%s\")>]" prop.Key.AsCodeJsonKey

            yield sprintf "    member __.Update%s (target : ComboProps, update : %s -> unit) =" memberName varType
            yield sprintf "        update <| target.Children.Get<%s> \"%s\"" varType prop.Key
            yield sprintf "        target"
        ]
    let getOperations (param : PrefabParam) =
        match meta with
        | :? ComboProps as combo ->
            [
                for prop in combo.Children.Value do
                    match prop with
                    | :? IViewProps as prop ->
                        yield getComboOperation param prop
                    | _ ->
                        ()
            ]|> List.concat
        | _ -> []
    interface IGenerator<PrefabParam> with
        member this.Generate param =
            [
                ["open Dap.Platform"]
                ["open Dap.Gui"]
                ["open Dap.Gui.Builder"]
                ["open Dap.Gui.Dsl.Prefabs"]
                [""]
                getBuilderHeader param
                getOperations param
                getBuilderFooter param
            ]|> List.concat
