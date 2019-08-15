module Handle.Rehydrator

open Kernel.Domain.User
open Kernel.Domain
open Kernel.Types
open Kernel.Domain.SuperAdmin
open Kernel.Domain.SuperAdmin
open Common.AsyncResult
open Write
open Common.AsyncResult
open Common.JsonConv
open System


let private getEntity applier stream = asyncResult {
    let! events = EventStore.readEvents stream

    return
        events 
        |> Seq.map (fun (number, eventData) -> number, Common.JsonConv.deserialize eventData)
        |> Seq.toArray
        |> Array.unzip
        |> fun (eventNumbers, domEvents) ->
            { Version = eventNumbers |> Array.max
              Object = domEvents |> applier } 
}
   
let getUser stream =
    getEntity (User.applyEvents User.User.Default) stream

let getRose stream =
    getEntity (SuperAdmin.applyEvents SuperAdmin.SuperAdmin.Default) stream


//
//let getRose stream =
//    desEvents<Domain.Rose.RoseEvent> (fun evs -> Domain.User.applyEvents Domain.User.User.Default evs) stream
//
//let create (events: DomainEvent list) =
//    match events.Head with
//    | DomainEvent.User _ ->
//        DomainTypes.User <| User.applyEvents User.Default (events |> Seq.map (function DomainEvent.User u -> u))
//    | DomainEvent.SuperAdmin _ ->
//        DomainTypes.SuperAdmin <| SuperAdmin.applyEvents SuperAdmin.Default (events |> Seq.map (function DomainEvent.SuperAdmin u -> u))