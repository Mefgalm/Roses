module Kernel.Domain.User

open System
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes

[<RequireQualifiedAccessAttribute>]
type UserEvent = 
    | UserCreated of Id: Guid * Email * Password * CreatedDate: DateTime
    | EmailUpdated of Id: Guid * NewEmail: Email

//let private toOk = Ok
//let private toError = Error
//  

type User =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: DateTime }
    with
        static member Default =
            { Id = Guid.Empty
              Email = Email.example
              Password = Password.example
              CreatedDate = DateTime.MinValue }

let createUser id email password repeatPassword getTime =
    if password = repeatPassword then
        Ok [UserEvent.UserCreated (id, email, password, getTime())]
    else
        Error "Passwords don't match" 

let updateEmail user newEmail =
    if user.Email <> newEmail then
        Ok [UserEvent.EmailUpdated (user.Id, newEmail)]
    else
        Ok []
    
let applyEvent user event =
    match event with
    | UserEvent.UserCreated (id, email, password, createdDate) ->
        { Id = id
          Email = email
          Password = password
          CreatedDate = createdDate } //TODO try re-write to copy-with operator
        
    | UserEvent.EmailUpdated (_, newEmail) ->
        { user with Email = newEmail }
        
        
let applyEvents user events =
    events |> Seq.fold applyEvent user
            