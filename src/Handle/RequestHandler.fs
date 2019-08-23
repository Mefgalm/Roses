module Handle.RequestHandler

open System
open Saga
open Akka.FSharp
open Kernel.Types
open AkkaCommon
open Common
open Common.Operators
open Handle
open Kernel.Domain.DomainTypes

[<RequireQualifiedAccessAttribute>]
type Request =
    | CreateUser of Email: string * Password: string * RepeatPassword: string
    | ChangeUserEmail of UserId: Guid * NewEmail: string
    | CreateSuperAdmin of Email: string * Password: string * RepeatPassword: string


let private mapSagaResponse response = function
    | SagaResponse.ForwardComplete -> Ok response
    | SagaResponse.BackwardComplete error -> Error error
    | SagaResponse.BackwardAbort error -> Error error
    | SagaResponse.Stop -> Ok None
    
let private mapEmptySagaResponse = mapSagaResponse None    

let requestHandler request = asyncResult {
    match request with
    | Request.CreateUser (email, password, repeatPassword) ->                    
                    
        let saga = runSaga ()

        let userId = Guid.NewGuid()

        let forwardCmd = Command.CreateUser (userId, email, password, repeatPassword)
        let backwardCmd = Command.RemoveUser userId
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))        
        
        return (saga <^? SagaCommand.Start) |> mapSagaResponse (Some userId)        
    | Request.ChangeUserEmail  (userId, newEmail) ->
        
        let saga = runSaga()
        
        let! userEntity = Rehydrator.getUser (userId.ToString())
        
        let forwardCmd = Command.ChangeUserEmail (userEntity, newEmail)
        let backwardCmd = Command.ChangeUserEmail (userEntity, userEntity.Object.Email |> Email.get)
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))
                
        return (saga <^? SagaCommand.Start) |> mapEmptySagaResponse

    | Request.CreateSuperAdmin (email, password, repeatPassword) ->
        let saga = runSaga ()
        
        let userId = Guid.NewGuid()
        
        let forwardCmd = Command.CreateSuperAdmin (userId, email, password, repeatPassword)
        let backwardCmd = Command.RemoveSuperAdmin userId
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))        
        
        return (saga <^? SagaCommand.Start) |> mapSagaResponse (Some userId)        
}