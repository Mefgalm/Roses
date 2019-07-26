module Kernel.RequestHandler

open System
open Saga
open Akka.FSharp
open Types
open AkkaCommon
open Common
open Common.Operators

[<RequireQualifiedAccessAttribute>]
type Request =
    | CreateUser of Email: string * Password: string * RepeatPassword: string
    | ChangeUserEmail of Id: Guid * NewEmail: string


let requestHandler eventHandlerActor request =
    match request with
    | Request.CreateUser (email, password, repeatPassword) ->                    
                    
        let saga = runSaga ()

        let userId = Guid.NewGuid()

        saga <! SagaCommand.AddForwardCommand ^ Forward ((Command.CreateUser (userId, password, repeatPassword, email)), Command.RemoveUser ^ userId)
        saga <! SagaCommand.AddForwardCommand ^ Forward ((Command.ChangeUserEmail (userId, "new-email@g.com"), Command.ChangeUserEmail (userId, email)))
        
        saga <^? SagaCommand.Start
    | _ ->
        SagaResponse.ForwardComplete