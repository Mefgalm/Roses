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
open HandleError

[<RequireQualifiedAccessAttribute>]
type Request =
    | CreateUser of Email: string * Password: string * RepeatPassword: string
    | ChangeUserEmail of UserId: Guid * NewEmail: string
    | CreateSuperAdmin of Email: string * Password: string * RepeatPassword: string
    | SignIn of Email: string * Password: string


type RequestInfo = 
    { Request : Request
      Token: string option }


let private mapSagaResponse<'a> (response: 'a) sagaResponse: Result<'a, CoreError.CoreError array> = 
    match sagaResponse with
    | SagaResponse.ForwardComplete -> Ok response
    | SagaResponse.BackwardComplete error -> Error error
    | SagaResponse.BackwardAbort error -> Error error
    | SagaResponse.Stop -> Ok response
    
let private mapEmptySagaResponse = mapSagaResponse None    

let requestHandler<'a> { Request = request; Token = token } = asyncResult {
    match request with
    | Request.CreateUser (email, password, repeatPassword) ->                    
        let saga = runSaga ()

        let userId = Guid.NewGuid()

        let forwardCmd = Command.CreateUser (userId, email, password, repeatPassword)
        let backwardCmd = Command.RemoveUser userId
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))
        
        return! (saga <^? SagaCommand.Start) |> mapSagaResponse (Some userId)

    | Request.ChangeUserEmail  (userId, newEmail) ->
        let saga = runSaga()
        
        let! userEntity = Rehydrator.getUser (userId.ToString())
        
        let forwardCmd = Command.ChangeUserEmail (userEntity, newEmail)
        let backwardCmd = Command.ChangeUserEmail (userEntity, userEntity.Object.Email |> Email.Get)
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))
                
        return! (saga <^? SagaCommand.Start) |> mapEmptySagaResponse

    | Request.CreateSuperAdmin (email, password, repeatPassword) ->
        let saga = runSaga ()
        
        let userId = Guid.NewGuid()
        
        let forwardCmd = Command.CreateSuperAdmin (userId, email, password, repeatPassword)
        let backwardCmd = Command.RemoveSuperAdmin userId
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))        
        
        return! (saga <^? SagaCommand.Start) |> mapSagaResponse (Some userId)

    | Request.SignIn (email, password) ->
        
        return! SagaResponse.Stop |> mapSagaResponse (Some "")
}