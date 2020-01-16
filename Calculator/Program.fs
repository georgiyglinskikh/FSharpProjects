open System
open System.Text.RegularExpressions
open System.Collections.Generic

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

let rec calculate (expression: string) =
    let mutable expressionLoc = expression
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
        Regex.Matches(expressionLoc, @"((\d+\.\d+)|(\d+))|(\+|\-|\/|((\*\*)|\*)|\^)")
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


[<EntryPoint>]
let main argv =
    if argv.Length = 1 then
        argv.[0]
        |> calculate
        |> Console.WriteLine
    else
        Console.ReadLine()
        |> calculate
        |> Console.WriteLine
    0
