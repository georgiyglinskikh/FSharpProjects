open System
open System.Collections.Generic
open SFML.Graphics
open SFML.Window

let Details = new List<List<int>>()

type Shapes =
    static member Circle = new CircleShape()
    static member Rectangle = new RectangleShape()

type ConnectionPoint() =
    member x.Childs = new List<ConnectionPoint>()
    member x.SendSignal(signal: float) =
        for i in x.Childs do
            i.SendSignal(signal)

[<AbstractClass>]
type Detail() =
    inherit Transformable()

    interface Drawable with
        member x.Draw(target: RenderTarget, states: RenderStates) = x.LocalDraw(target, states)

    member x.InputPoint = ConnectionPoint()
    member x.OutputPoints = new List<ConnectionPoint>()

    abstract HandleSignal: float -> unit

    default x.HandleSignal(value) =
        for i in 0 .. (x.OutputPoints.Count - 1) do
            x.OutputPoints.[i].SendSignal(value)

    abstract LocalDraw: RenderTarget * RenderStates -> unit
    default x.LocalDraw(target, states) = ()


type Scene() =
    inherit Transformable()

    interface Drawable with
        member x.Draw(target: RenderTarget, states: RenderStates) = ()

    member x.HandleMouse(args: MouseButtonEventArgs) = ()

    member x.Update() = ()


[<EntryPoint>]
let main argv =
    let window = new RenderWindow(VideoMode(800u, 600u), "Electricity")

    window.Closed.Add(fun _ -> window.Close())

    let scene = new Scene()

    window.MouseButtonPressed.Add(fun args ->
        scene.HandleMouse args
        scene.Update())

    while window.IsOpen do
        window.DispatchEvents()

        window.Clear()

        window.Draw(scene)

        window.Display()
    0
