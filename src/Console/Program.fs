// Learn more about F# at http://fsharp.org

open Write
open EventStore.ClientAPI
open Handle
    
//let getRose stream =
//    desEvents<Domain.Rose.RoseEvent> (fun evs -> Domain.User.applyEvents Domain.User.User.Default evs) stream

//let restoreUser events =
//    Domain.User.applyEvents Domain.User.User.Default events


type TestA = { Id : string }
type TestB = { Id : string; Name: string }


[<EntryPoint>]
let main argv =
    let createUserRequest = (RequestHandler.Request.CreateUser  ("mefgalm2@gmail.com", "123", "123"))
    
    match RequestHandler.requestHandler createUserRequest |> Async.RunSynchronously with
    | Ok result -> sprintf "%A" result
    | Error error -> error
    |> printfn "result is -> %s"

    0