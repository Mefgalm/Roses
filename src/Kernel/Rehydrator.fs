module Kernel.Rehydrator

open Kernel.Domain.User
open Kernel.Domain
open Kernel.Domain.SuperAdmin
open Kernel.Domain.SuperAdmin
open Common.AsyncResult



    
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