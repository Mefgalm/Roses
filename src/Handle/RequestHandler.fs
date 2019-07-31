module Handle.RequestHandler

open System
open Saga
open Akka.FSharp
open Kernel.Types
open AkkaCommon
open Common
open Common.Operators

[<RequireQualifiedAccessAttribute>]
type Request =
    | CreateUser of Email: string * Password: string * RepeatPassword: string
    | ChangeUserEmail of UserId: Guid * NewEmail: string


let requestHandler request =
    match request with
    | Request.CreateUser (email, password, repeatPassword) ->                    
                    
        let saga = runSaga ()

        let userId = Guid.NewGuid()

        saga <! SagaCommand.AddForwardCommand ^ Forward ((Command.CreateUser (userId, email, password, repeatPassword)), Command.RemoveUser ^ userId)        
        
        saga <^? SagaCommand.Start
    | Request.ChangeUserEmail  (userId, newEmail) ->
        
        let saga = runSaga()
        
        //get user from ES    
        SagaResponse.ForwardComplete
    