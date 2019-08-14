// Learn more about F# at http://fsharp.org

open Akka.IO
open Common
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
open Newtonsoft.Json
open Newtonsoft.Json.Converters
    
//let getRose stream =
//    desEvents<Domain.Rose.RoseEvent> (fun evs -> Domain.User.applyEvents Domain.User.User.Default evs) stream

//let restoreUser events =
//    Domain.User.applyEvents Domain.User.User.Default events

[<EntryPoint>]
let main argv =

 
    
    let createUserRequest = (RequestHandler.Request.CreateUser  ("mefgalm2@gmail.com", "123", "123"))
    
    let response = RequestHandler.requestHandler createUserRequest
    
    printfn "result is %A" response
    
   
    match Rehydrator.getUser "b27edfe7-88e0-4e49-8baa-14ee006bed6b" |> Async.RunSynchronously with
    | Ok user -> printfn "User is %A" user
    | Error str -> printfn "not found"

    0