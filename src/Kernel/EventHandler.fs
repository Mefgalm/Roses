module Kernel.EventHandler

open AkkaCommon
open Akka.FSharp
open Kernel.Types
open Kernel.Domain.User
open Types
open Common

let eventStore () = Ok ()

let mongoDb () = Ok ()


let eventHandler event =
    match event with 
    | DomainEvent.User (UserEvent.UserCreated (id, firstName, email) as e)-> 
         Result.reduceResults [eventStore(); mongoDb ()]
        
    | DomainEvent.User (UserEvent.EmailUpdated (id, newEmail) as e) ->
         Result.reduceResults [eventStore(); mongoDb ()]
         

let eventHandlerActor =
    spawn system "event-handler-actor" 
        (fun mailbox -> 
            let rec loop () = actor {

                let! event = mailbox.Receive()

                eventHandler event |> ignore //I'm totally sure that it's ok (づ｡◕‿‿◕｡)づ
            }
            loop ())