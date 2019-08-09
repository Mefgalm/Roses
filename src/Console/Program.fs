// Learn more about F# at http://fsharp.org

open Akka.IO
open System
open System
open System.IO
open System.IO
open MBrace.FsPickler.Json
open Write
open Kernel
open EventStore.ClientAPI
open EventStore.ClientAPI
open FSharp.Control.Tasks
open Kernel.Types
open Handle
open Common.AsyncResult
    
//let getRose stream =
//    desEvents<Domain.Rose.RoseEvent> (fun evs -> Domain.User.applyEvents Domain.User.User.Default evs) stream

//let restoreUser events =
//    Domain.User.applyEvents Domain.User.User.Default events

type User = { Id: string }

type Admin = { Age: int }

[<EntryPoint>]
let main argv =
    
//    let createUserRequest = (RequestHandler.Request.CreateUser  ("mefgalm2@gmail.com", "123", "123"))
//    
//    let response = RequestHandler.requestHandler createUserRequest
//    
//    printfn "result is %A" response
//    
//    match (Write.EventStore.readDomainEvents "5e86df46-5a9d-4a57-80be-698390bf65f6") |> Async.RunSynchronously with
//    | Ok res -> printfn "res is %A" res
//    | Error str -> ()

    
    
    0       