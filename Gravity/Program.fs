open System
open System.Collections.Generic
open SFML.Graphics
open SFML.Window
open SFML.System

let mutable winSize = Vector2f(800.f, 600.f)

let circleRadiuses = new List<float32>()
let circlePositions = new List<Vector2f>()
let circleSpeed = new List<Vector2f>()

let mutable selectedCircle = 0

let G = 6.01e-2f

let getDistance (point0: Vector2f, point1: Vector2f) =
    sqrt (((point0.X - point1.X) ** 2.f) + ((point0.Y - point1.Y) ** 2.f))

let findNearestPointTo point =
    let mutable minI = Option<int>.None
    let mutable minDistance = Option<float32>.None
    for i in 0 .. (circlePositions.Count - 1) do
        let distance = getDistance (point, circlePositions.[i])
        if minDistance.IsNone || distance < minDistance.Value then
            minDistance <- Some(Value = distance)
            minI <- Some(i)

    minI

let isInRadius radius (point0: Vector2f, point1: Vector2f) = getDistance (point0, point1) <= radius

let calcA (circle0: {| Radius: float32; Position: Vector2f |}, circle1: {| Radius: float32; Position: Vector2f |}) =
    G
    * ((circle0.Radius * circle1.Radius) / (getDistance (circle0.Position, circle1.Position) ** 2.f)) // F = G * ((m1 * m2) / r ^ 2)

let realizeButton (args: MouseButtonEventArgs) =
    let nearestPoint = findNearestPointTo (Vector2f(float32 args.X, float32 args.Y))
    match args.Button with
    | Mouse.Button.Right ->
        if nearestPoint.IsSome
           && isInRadius circleRadiuses.[nearestPoint.Value]
                  (circlePositions.[nearestPoint.Value], Vector2f(float32 args.X, float32 args.Y)) then
            circlePositions.RemoveAt(nearestPoint.Value)
            circleRadiuses.RemoveAt(nearestPoint.Value)
            circleSpeed.RemoveAt(nearestPoint.Value)
        else
            circlePositions.Add(Vector2f(float32 args.X, float32 args.Y))
            circleRadiuses.Add(10.f)
            circleSpeed.Add(Vector2f(0.f, 0.f))
    | Mouse.Button.Left ->
        if circlePositions.Count > 0 then
            selectedCircle <- (findNearestPointTo (Vector2f(float32 args.X, float32 args.Y))).Value
    | _ -> ()

let resizeWithWheel (args: MouseWheelScrollEventArgs) =
    if circlePositions.Count > 0 then circleRadiuses.[selectedCircle] <- circleRadiuses.[selectedCircle] + args.Delta


let update() =
    // Move all circles
    for i in 0 .. (circlePositions.Count - 1) do
        circlePositions.[i] <- circlePositions.[i] + circleSpeed.[i]

    if circlePositions.Count > 1 then
        let bounced = new List<int>()
        for i in 0 .. (circlePositions.Count - 1) do
            for j in 0 .. (circlePositions.Count - 1) do
                if i <> j then
                    // Add gravity
                    let t = circlePositions.[i] - circlePositions.[j]
                    let a = t.X
                    let b = t.Y
                    let c = getDistance (circlePositions.[i], circlePositions.[j])

                    let A =
                        calcA
                            ({| Radius = circleRadiuses.[i]
                                Position = circlePositions.[i] |},
                             {| Radius = circleRadiuses.[j]
                                Position = circlePositions.[j] |})
                    circleSpeed.[i] <- Vector2f(circleSpeed.[i].X + -(A * (a / c)) / circleRadiuses.[i], circleSpeed.[i].Y + -(A * (b / c)) / circleRadiuses.[i])

                    // Add bounces from each circle
                    (* if getDistance (circlePositions.[i], circlePositions.[j]) < (circleRadiuses.[i] + circleRadiuses.[j]) then
                        circleSpeed.[i] <- -circleSpeed.[i] *)
                    if (not (bounced.Contains(i))) && (not (bounced.Contains(j))) && c < (circleRadiuses.[i] + circleRadiuses.[j]) then
                        let tempVector = circleSpeed.[i]
                        circleSpeed.[i] <- Vector2f(circleSpeed.[j].X * (circleRadiuses.[j] / circleRadiuses.[i]), circleSpeed.[j].Y * (circleRadiuses.[j] / circleRadiuses.[i]))
                        circleSpeed.[j] <- Vector2f(tempVector.X * (circleRadiuses.[i] / circleRadiuses.[j]), tempVector.Y * (circleRadiuses.[i] / circleRadiuses.[j]))
                        bounced.AddRange([i; j])
                    // if (not (bounced.Contains(i))) && (not (bounced.Contains(j))) && c < (circleRadiuses.[i] + circleRadiuses.[j]) then
                    //     let tempVector = circleSpeed.[i]
                    //     circleSpeed.[i] <- Vector2f(circleSpeed.[j].X * (circleRadiuses.[j] / circleRadiuses.[i]) * ((circleSpeed.[i].X) / (circleSpeed.[j].X)), circleSpeed.[j].Y * (circleRadiuses.[j] / circleRadiuses.[i]) * ((circleSpeed.[i].X) / (circleSpeed.[j].X)))
                    //     circleSpeed.[j] <- Vector2f(tempVector.X * (circleRadiuses.[i] / circleRadiuses.[j]) * ((circleSpeed.[j].X) / (circleSpeed.[i].X)), tempVector.Y * (circleRadiuses.[i] / circleRadiuses.[j]) * (circleSpeed.[j].Y / circleSpeed.[i].Y))
                    //     bounced.AddRange([i; j])



    // Add bounces from border
    for i in 0 .. (circlePositions.Count - 1) do
        if (circlePositions.[i].X - circleRadiuses.[i]) < 0.f
           || (circlePositions.[i].X + circleRadiuses.[i]) > winSize.X then
            circleSpeed.[i] <- Vector2f(-circleSpeed.[i].X, circleSpeed.[i].Y)
        if (circlePositions.[i].Y - circleRadiuses.[i]) < 0.f
           || (circlePositions.[i].Y + circleRadiuses.[i]) > winSize.Y then
            circleSpeed.[i] <- Vector2f(circleSpeed.[i].X, -circleSpeed.[i].Y)

[<EntryPoint>]
let main argv =
    let window = new RenderWindow(VideoMode(uint32 winSize.X, uint32 winSize.Y), "Gravity")
    window.SetVerticalSyncEnabled true

    window.Closed.Add(fun _ -> window.Close())
    window.Resized.Add(fun _ -> winSize <- Vector2f(float32 window.Size.X, float32 window.Size.Y); window.SetView(new View(FloatRect(0.f, 0.f, winSize.X, winSize.Y)));)

    window.MouseButtonPressed.Add(realizeButton)
    window.MouseWheelScrolled.Add(resizeWithWheel)

    let circle = new CircleShape(FillColor = Color.White)

    while window.IsOpen do
        window.DispatchEvents()

        window.Clear()

        for i in 0 .. (circleRadiuses.Count - 1) do
            circle.Radius <- circleRadiuses.[i]
            circle.Position <- circlePositions.[i]
            circle.Origin <- Vector2f(circleRadiuses.[i], circleRadiuses.[i])
            window.Draw(circle)

        window.Display()

        update()
    0
