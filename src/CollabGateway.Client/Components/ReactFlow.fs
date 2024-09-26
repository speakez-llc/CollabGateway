module ReactFlow

open Fable.Core.JsInterop
open Fable.Core
open Feliz

let reactFlow : obj = importDefault "reactflow" // Updated import
let removeElements : obj = import "removeElements" "reactflow"
let addEdge : obj = import "addEdge" "reactflow"

[<Erase>]
/// This interface allows us to stop adding random props to the react flow.
type IReactFlowProp = interface end

// Define Node type
type Node = {
    id: string
    data: obj
    position: {| x: float; y: float |}
    ``type``: string option
}

// Define Edge type
type Edge = {
    id: string
    source: string
    target: string
    ``type``: string option
    animated: bool option
}

// Some sample types you can use for setting properties on elements.
type EdgeType = Bezier | Straight | Step | SmoothStep member this.Value = this.ToString().ToLower()
type ArrowHead = Arrow | ArrowClosed member this.Value = this.ToString().ToLower()
type NodeType = Input | Output | Default member this.Value = this.ToString().ToLower()

let funcToTuple handler = System.Func<_,_,_>(fun a b -> handler(a,b))

// The !! below is used to "unsafely" expose a prop into an IReactFlowProp.
[<Erase>]
type ReactFlow =
    /// Creates a new ReactFlow component.
    static member inline create (props:IReactFlowProp seq) = Interop.reactApi.createElement (reactFlow, createObj !!props)

    /// Provides the child elements in a flow.
    static member inline elements (elements:_ array) : IReactFlowProp = !!("elements" ==> elements)
    static member inline onElementClick  (handler:(obj*obj) -> unit) : IReactFlowProp = !!("onElementClick" ==> funcToTuple handler)
