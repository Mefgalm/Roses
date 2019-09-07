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
open Kernel
open CoreError


let private waitResults (tasks: seq<Async<Result<unit, CoreError array>>>) =
    tasks
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Common.Result.reduceResults

let private mapEvents minVersion mapper =
    Seq.mapi (fun version event -> eventHandler (minVersion + (version |> int64)) (event |> mapper))
    
let private zeroMapEvents mapper =
    mapEvents -1L mapper

let handleCommand command: Result<unit, CoreError array> = result {
    match command with
    | Command.CreateUser (userId, email, password, repeatPassword, roles) ->
        let! email = Email.Create email |> domainToCore
        let! password = Password.Create password |> domainToCore
        let! repeatPassword = Password.Create repeatPassword |> domainToCore
        let! createdDate = CreatedDate.Create DateTime.UtcNow
        
        let! events = User.createUser userId email password repeatPassword roles createdDate |> domainToCore

        return! events
                |> zeroMapEvents DomainEvent.User
                |> waitResults
                
    | Command.RemoveUser userId ->
        return! Ok ()

    //| Command.CreateSuperAdmin (userId, email, password, repeatPassword) ->
    //    let! email = Email.create email
    //    let! password = Password.create password
    //    let! repeatPassword = Password.create repeatPassword
    //    let! createdDate = CreatedDate.create DateTime.UtcNow
        
    //    let! events = SuperAdmin.create userId email password repeatPassword createdDate 
                
    //    return! events
    //        |> zeroMapEvents DomainEvent.SuperAdmin
    //        |> waitResults
    | Command.SignIn (user, email, password) ->
        let! events = User.signIn user email password |> domainToCore

        return! events
                |> zeroMapEvents DomainEvent.User
                |> waitResults

    | _ -> return! Ok ()

    //| Command.RemoveSuperAdmin userId ->
    //    let! events = SuperAdmin.
}