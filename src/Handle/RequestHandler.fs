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
open Kernel.Domain.User
open CoreError
open Read.Types
open JWTService

[<RequireQualifiedAccessAttribute>]
type Request =
    | CreateSuperAdminUser of Email: string * Password: string * RepeatPassword: string
    | ChangeUserEmail of UserId: Guid * NewEmail: string
    | SignIn of Email: string * Password: string


type RequestInfo = 
    { Request : Request
      Token: string option }


let private mapSagaResponse response sagaResponse = 
    match sagaResponse with
    | SagaResponse.ForwardComplete -> Ok (box response)
    | SagaResponse.BackwardComplete error -> Error error
    | SagaResponse.BackwardAbort error -> Error error
    | SagaResponse.Stop -> Ok (box response)
    

let private mapEmptySagaResponse = mapSagaResponse None    


let private readToUser (userRead: UserRead) = Common.Result.result { //TODO I don't like it. Bad smell
        let! email = Email.Create userRead.Email |> domainToCore
        let! password = Password.Create userRead.Password |> domainToCore
        let! createdDate = CreatedDate.Create DateTime.UtcNow

        return 
            { User.Id = Guid.Parse(userRead.Id)
              Email = email
              Password = password
              CreatedDate = createdDate
              Roles = [] }
    }


 //TODO move it to settings
let secret = JWTService.Secret.Create "1234567890qwerty"
let issuer = JWTService.Issuer.Create "rosesIssuer"
let audience = JWTService.Audience.Create "rosesAudience"


let private rosesJwtGenerator = 
    JWTService.generate secret issuer audience (JWTService.DaysToExpire.Create 30.) DateTime.Now


let private validate token =
    if JWTService.validateToken secret issuer audience token 
    then Ok ()
    else Error [|HandleError.Unauthorized|]

let requestHandler<'a> { Request = request; Token = token } = asyncResult {
    match request with
    | Request.CreateSuperAdminUser (email, password, repeatPassword) ->                    
        let saga = runSaga ()

        let userId = Guid.NewGuid()

        let forwardCmd = Command.CreateUser (userId, email, password, repeatPassword, [Role.SuperAdmin])
        let backwardCmd = Command.RemoveUser userId
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))
        
        return! (saga <^? SagaCommand.Start) |> mapSagaResponse (Some userId)

    | Request.ChangeUserEmail (userId, newEmail) ->
        let saga = runSaga()
        
        let! userEntity = Rehydrator.getUser (userId.ToString())

        do! validate token |> handleToCore

        let forwardCmd = Command.ChangeUserEmail (userEntity, newEmail)
        let backwardCmd = Command.ChangeUserEmail (userEntity, userEntity.Object.Email |> Email.Get)
        
        saga <! SagaCommand.AddForwardCommand (Forward (forwardCmd, backwardCmd))
            
        return! (saga <^? SagaCommand.Start) |> mapEmptySagaResponse

    | Request.SignIn (email, password) ->
        let saga = runSaga()

        let! userRead = (Read.UserReadDb.getUserByEmail email |> Async.Map readToCore)

        let! user = userRead |> readToUser

        let forwardCmd = Command.SignIn (user, email, password)
        saga <! SagaCommand.AddForwardCommand(Forward (forwardCmd, Command.DoNothing))

        let tokenPayload = 
            { Id = user.Id
              Email = user.Email
              Roles = userRead.Roles }

        return! (saga <^? SagaCommand.Start) |> mapSagaResponse (rosesJwtGenerator tokenPayload)
    //| Request.SignIn (email, password) ->

    //    let saga = runSuga()

    //    //let! userEntity = Rehydrator.getUser ()

}