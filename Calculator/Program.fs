open System
open System.Text.RegularExpressions
open System.Collections.Generic

let value operator =
    match operator with
    | "+" | "-" -> 1
    | "*" | "/" -> 2
    | "**" -> 3
    | "(" | ")" -> -1
    | _ -> 0

let perform opertator num0 num1 =
    match opertator with
    | "+" -> num0 + num1
    | "-" -> num0 - num1
    | "*" -> num0 * num1
    | "/" -> num0 / num1
    | "**" -> int ((float num0) ** (float num1))
    | _ -> 0

let calculate expression =
    let stuff = Regex.Matches(expression, @"(\d+)|(\+|\-|\*|\/|(\*\*))") |> ( fun col -> [ for i in col -> i.Value] )
    let allNumbers = Queue<int>()
    stuff |> List.filter (fun el -> value el = 0) |> List.map int |> List.iter allNumbers.Enqueue 
    let allOperators = Queue<string>()
    stuff |> List.filter (fun el -> value el <> 0) |> List.iter allOperators.Enqueue

    let currentNum = Stack()
    let currentOp = Stack()

    currentNum.Push (allNumbers.Dequeue())
    currentOp.Push (allOperators.Dequeue())
    currentNum.Push (allNumbers.Dequeue())

    while currentOp.Count > 0 do
        let op1 = currentOp.Pop()
        if allOperators.Count > 0 then
            let op0 = allOperators.Dequeue()
            if value op0 < value op1 then
                currentNum.Push (perform op1 (currentNum.Pop()) (currentNum.Pop()))
            else
                currentOp.Push(op1)
            currentNum.Push(allNumbers.Dequeue())
            currentOp.Push(op0)
        else
            currentNum.Push (perform op1 (currentNum.Pop()) (currentNum.Pop()))

    currentNum.Pop()


[<EntryPoint>]
let main argv =
    Console.ReadLine()
    |> calculate
    |> Console.WriteLine
    0 
