open System
open System.Text.RegularExpressions
open System.Collections.Generic
open SFML.Graphics
open SFML.Window
open SFML.System

let value operator =
    match operator with
    | "+"
    | "-" -> 1
    | "*"
    | "/" -> 2
    | "**" -> 3
    | "("
    | ")" -> -1
    | _ -> 0

let perform opertator num0 num1 =
    match opertator with
    | "+" -> num1 + num0
    | "-" -> num1 - num0
    | "*" -> num1 * num0
    | "/" -> num1 / num0
    | "**" -> num1 ** num0
    | _ -> 0.

let rec calculate expression =
    let mutable expressionLoc = string expression
    while expressionLoc.Contains "(" || expressionLoc.Contains ")" do
        expressionLoc <-
            (if (expressionLoc.IndexOf "(") > 0
             then expressionLoc.[..((expressionLoc.IndexOf "(") - 1)]
             else "")
            + string (calculate (expressionLoc.[((expressionLoc.IndexOf "(") + 1)..((expressionLoc.IndexOf ")") - 1)]))
            + (if (expressionLoc.IndexOf ")") < expressionLoc.Length
               then expressionLoc.[((expressionLoc.IndexOf ")") + 1)..]
               else "")

    let stuff =
        Regex.Matches(expressionLoc, @"((\d+\.\d+)|(\d+))|(\+|\-|\/|((\*\*)|\*))")
        |> (fun col -> [ for i in col -> i.Value ])

    let allNumbers = Queue<float>()
    stuff
    |> List.filter (fun el -> value el = 0)
    |> List.map float
    |> List.iter allNumbers.Enqueue
    let allOperators = Queue<string>()
    stuff
    |> List.filter (fun el -> value el <> 0)
    |> List.iter allOperators.Enqueue

    if allOperators.Count <> 0 then
        let currentNum = Stack()
        let currentOp = Stack()

        currentNum.Push(allNumbers.Dequeue())
        currentOp.Push(allOperators.Dequeue())
        currentNum.Push(allNumbers.Dequeue())

        while currentOp.Count > 0 do
            let op1 = currentOp.Pop()
            if allOperators.Count > 0 then
                let op0 = allOperators.Dequeue()
                if value op0 < value op1
                then currentNum.Push(perform op1 (currentNum.Pop()) (currentNum.Pop()))
                else currentOp.Push(op1)
                currentNum.Push(allNumbers.Dequeue())
                currentOp.Push(op0)
            else
                currentNum.Push(perform op1 (currentNum.Pop()) (currentNum.Pop()))

        currentNum.Pop()
    else
        allNumbers.Dequeue()

let draw expression =
    let window = new RenderWindow(VideoMode(800u, 600u), "Visualize")
    window.SetVerticalSyncEnabled true

    window.Closed.Add(fun args -> window.Close())
    window.Resized.Add(fun args ->
        window.Clear()
        window.Draw
            ([| for i in 0 .. int window.Size.X ->
                    Vertex
                        (Position =
                            Vector2f
                                (float32 i,
                                 Regex.Replace(expression, "x", string i)
                                 |> calculate
                                 |> float32), Color = Color.White) |], PrimitiveType.LineStrip)

        window.Display())

    window.Clear()
    window.Draw
        ([| for i in 0 .. int window.Size.X ->
                Vertex
                    (Position =
                        Vector2f
                            (float32 i,
                             Regex.Replace(expression, "x", string i)
                             |> calculate
                             |> float32), Color = Color.White) |], PrimitiveType.LineStrip)

    window.Display()

    while window.IsOpen do
        window.DispatchEvents()

let visualize (expression: string) =
    if expression.Contains "x" then
        draw expression
    else
        expression
        |> calculate
        |> Console.WriteLine

[<EntryPoint>]
let main argv =
    let expression = argv.[0]

    visualize expression
    0
