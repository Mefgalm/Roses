module Handle.AkkaCommon

open Akka.Actor
open Akka.FSharp

let system = System.create "system" (Configuration.load())

let inline (<^?) (actor: IActorRef) message: 'a =
    actor.Ask(message) |> Async.AwaitTask |> Async.RunSynchronously :?> 'a