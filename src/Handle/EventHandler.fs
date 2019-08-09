module Handle.EventHandler

open AkkaCommon
open FSharp.Control.Tasks

open System
open System.Threading
open Handle
open Akka.FSharp
open Kernel.Types
open Kernel.Domain.User
open Kernel.Types
open Common
open Kernel.Domain.DomainTypes
open Write
open System.Threading.Tasks
open Read

let eventStore nextVersion event =
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, _, _, _) as e) ->
        EventStore.writeEvent<UserEvent> id nextVersion e
        
    | DomainEvent.User (UserEvent.EmailUpdated (id, _) as e) ->
        EventStore.writeEvent<UserEvent> id nextVersion e

let mongoDb event = asyncResult {
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, email, password, createdDate)) ->
        return! ReadDb.addUser (id.ToString()) (Email.get email) (Password.get password) (CreatedDate.get createdDate)
    | DomainEvent.User (UserEvent.EmailUpdated (id, newEmail)) ->
        let strId = id.ToString()
        
        let! userRead = ReadDb.getUser strId
        
        do! ReadDb.updateUser strId { userRead with Email = (Email.get newEmail) }
        
        return ()
}


let eventHandler nextVersion event = async {
    let! res = [eventStore nextVersion event; mongoDb event] |> Async.Parallel
    
    return res |> Result.reduceResults
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