module Handle.CommandHandler

open System
open AkkaCommon
open Kernel.Types
open Kernel.Domain
open Akka.FSharp
open EventHandler
open Common.Result
open Common.Operators
open Kernel.Types
open System.Threading.Tasks

let private waitResults (tasks: seq<Task<Result<unit, string>>>) =
    tasks
    |> Task.WhenAll
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> Common.Result.reduceResults

let handleCommand command =
    match command with
    | Command.CreateUser (userId, email, password, repeatPassword) ->
        User.createUser userId email password repeatPassword (fun () -> DateTime.UtcNow)
        |> Result.bind (
            Seq.mapi(fun version event -> eventHandler (version |> int64) (event |> DomainEvent.User))            
            >> waitResults)
                  
    | Command.ChangeUserEmail (userEntity, newEmail) ->
        let { Version = currentVersion; Object = userObj } = userEntity
        
        User.updateEmail userObj newEmail
        |> Result.bind (
            Seq.mapi(fun version event -> eventHandler (currentVersion + (version |> int64)) (event |> DomainEvent.User))
            >> waitResults)
    | Command.RemoveUser userId ->
        Ok ()    
