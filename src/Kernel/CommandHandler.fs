module Kernel.CommandHandler

open AkkaCommon
open Types
open Kernel.Domain
open Akka.FSharp
open EventHandler
open Common.Result
open Common.Operators

let saveToWrite domainEvent =
    async { return Ok () }
    
let saveToRead domainEvent =
    async { return Ok () }

let handleCommand command = result {
    match command with
    | Command.CreateUser (userId, email, password, repeatPassword) ->
        let events = User.createUser userId email password repeatPassword
        
        return!
            [saveToWrite ()
             saveToRead ()]
            |> Async.Parallel
            |> Async.RunSynchronously
            |> reduceResults                
    | Command.ChangeUserEmail (userId, newEmail) ->
        return! Ok ()
    | Command.RemoveUser userId ->
        return! Ok ()    
}