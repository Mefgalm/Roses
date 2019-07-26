module Kernel.Domain.User

open System
open Kernel.Domain.DomainTypes

[<RequireQualifiedAccessAttribute>]
type UserEvent = 
    | UserCreated of Id: Guid * Email * Password * CreatedDate: DateTime
    | EmailUpdated of Id: Guid * NewEmail: Email

let private toOk = Result<UserEvent list, string>.Ok
let private toError = Result<UserEvent list, string>.Error
  
type User =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: DateTime }

let createUser id email password repeatPassword getTime =
    if password = repeatPassword then
        [UserEvent.UserCreated (id, email, password, getTime())] |> toOk
    else
        "Passwords don't match" |> toError

let updateEmail user newEmail =
    if user.Email <> newEmail then
        [UserEvent.EmailUpdated (user.Id, newEmail)] |> toOk
    else
        [] |> toOk
    
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