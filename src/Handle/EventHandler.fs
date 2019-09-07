module Handle.EventHandler

open AkkaCommon
open FSharp.Control.Tasks

open Handle
open Kernel.Types
open Kernel.Domain.User
open Common
open Kernel.Domain.DomainTypes
open Write
open Read
open Read.Types
open CoreError

let eventStore nextVersion event =
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, _, _, _, _) as e)    
    | DomainEvent.User (UserEvent.EmailUpdated (id, _) as e) ->
        EventStore.writeEvent<UserEvent> id nextVersion e
        

let mongoDb event = asyncResult {
    match event with
    | DomainEvent.User (UserEvent.UserCreated (id, email, password, createdDate, roles)) ->
              
        let userRead =
            { UserRead.Id = id.ToString()
              Email = email |> Email.Get
              Password = password |> Password.Get
              Roles = roles |> List.map(fun x -> x.ToString())
              CreatedDate = createdDate |> CreatedDate.Get }
        
        return! ReadDb.addEntity<UserRead> userRead
                    
    | DomainEvent.User (UserEvent.EmailUpdated (id, newEmail)) ->
        let strId = id.ToString()
        
        let! userRead = UserReadDb.getUser strId
        
        do! ReadDb.updateEntity { userRead with Email = (Email.Get newEmail) }
        
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