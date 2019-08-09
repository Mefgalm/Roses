module Handle.CommandHandler

open System
open AkkaCommon
open Kernel.Types
open Kernel.Domain
open Akka.FSharp
open EventHandler
open Common
open Common.Result
open Kernel.Domain.DomainTypes

let private waitResults (tasks: seq<Async<Result<unit, string>>>) =
    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Common.Result.reduceResults

let private mapEvents minVersion mapper =
    Seq.mapi (fun version event -> eventHandler (minVersion + (version |> int64)) (event |> mapper))
    
let private zeroMapEvents mapper =
    mapEvents -1L mapper

let handleCommand command = result {
    match command with
    | Command.CreateUser (userId, email, password, repeatPassword) ->
        let! email = Email.create email
        let! password = Password.create password
        let! repeatPassword = Password.create repeatPassword
        let! createdDate = CreatedDate.create DateTime.UtcNow
        
        let! events = User.createUser userId email password repeatPassword createdDate 
                
        return! events
            |> zeroMapEvents DomainEvent.User
            |> waitResults
                  
    | Command.ChangeUserEmail ({ Version = currentVersion; Object = userObj }, newEmail) ->
        let! newEmail = Email.create newEmail
        
        let! events = User.updateEmail userObj newEmail 
        
        return! events
            |> mapEvents currentVersion DomainEvent.User
            |> waitResults
    | Command.RemoveUser userId ->
        return! Ok ()
}