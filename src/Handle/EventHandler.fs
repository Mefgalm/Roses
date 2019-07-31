module Handle.EventHandler

open AkkaCommon
open FSharp.Control.Tasks

open System.Threading
open Handle
open Akka.FSharp
open Kernel.Types
open Kernel.Domain.User
open Kernel.Types
open Common
open Write
open System.Threading.Tasks


[<Literal>]
let user = "User"

let eventStore nextVersion event =
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, _, _, _) as e) ->
        EventStore.writeEvent id user nextVersion e
        
    | DomainEvent.User (UserEvent.EmailUpdated (id, _) as e) ->
        EventStore.writeEvent id user nextVersion e

let mongoDb () = async {    
    return Ok ()
}


let eventHandler nextVersion event = async {
    let! results = [eventStore nextVersion event; mongoDb ()] |> Async.Parallel
    
    return results |> Result.reduceResults    
} 
         

//let eventHandlerActor =
//    spawn system "event-handler-actor" 
//        (fun mailbox -> 
//            let rec loop () = actor {
//
//                let! event = mailbox.Receive()
//
//                eventHandler event |> ignore //I'm totally sure that it's ok (づ｡◕‿‿◕｡)づ
//            }
//            loop ())