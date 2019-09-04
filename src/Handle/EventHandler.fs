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
open Kernel.Domain.SuperAdmin
open Write
open Read
open Read.Types
open CoreError

let eventStore nextVersion event =
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, _, _, _) as e)    
    | DomainEvent.User (UserEvent.EmailUpdated (id, _) as e) ->
        EventStore.writeEvent<UserEvent> id nextVersion e
        
    | DomainEvent.SuperAdmin (SuperAdminEvent.Created (id, _, _, _) as e) ->
        EventStore.writeEvent<SuperAdminEvent> id nextVersion e

    | DomainEvent.SuperAdmin ((SuperAdminEvent.Removed id) as e) ->
        EventStore.writeEvent<SuperAdminEvent> id nextVersion e

let mongoDb event = asyncResult {
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, email, password, createdDate)) ->
              
        let userRead =
            { UserRead.Id = id.ToString()
              Email = email |> Email.Get
              Password = password |> Password.Get
              CreatedDate = createdDate |> CreatedDate.Get }
        
        return! ReadDb.addEntity<UserRead> userRead
                    
    | DomainEvent.User (UserEvent.EmailUpdated (id, newEmail)) ->
        let strId = id.ToString()
        
        let! userRead = ReadDb.getUser strId
        
        do! ReadDb.updateEntity { userRead with Email = (Email.Get newEmail) }
        
        return ()
        
    | DomainEvent.SuperAdmin (SuperAdminEvent.Created (id, email, password, createdDate)) ->
        
        let superAdminRead =
            { SuperAdminRead.Id = id.ToString()
              Email = email |> Email.Get
              Password = password |> Password.Get
              CreatedDate = createdDate |> CreatedDate.Get
              Status = SuperAdminStatus.Active }
                                
        return! ReadDb.addEntity<SuperAdminRead> superAdminRead

    | DomainEvent.SuperAdmin (SuperAdminEvent.Removed id) ->
        
        let! superAdminRead = ReadDb.getSuperAdmin (id.ToString())

        do! ReadDb.updateEntity { superAdminRead with Status = SuperAdminStatus.Removed }

        return ()
}

let eventHandler nextVersion event = async {
    let! res = [eventStore nextVersion event |> Async.Map writeToCore
                mongoDb event |> Async.Map readToCore] 
                |> Async.Parallel
    
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