module Kernel.Domain.User

open System
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes
open Newtonsoft.Json
open Kernel


[<RequireQualifiedAccessAttribute>]
type UserEvent = 
    | UserCreated of Id: Guid * Email * Password * CreatedDate
    | EmailUpdated of Id: Guid * NewEmail: Email


type User =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: CreatedDate }
    with
        static member Default =
            { Id = Guid.Empty
              Email = Email.Example
              Password = Password.Example
              CreatedDate = CreatedDate.Example }

let createUser id email password repeatPassword createdDate =
    if password = repeatPassword then
        Ok [|UserEvent.UserCreated (id, email, password, createdDate)|]
    else
        Error [|DomainError.PassworAndRepeatPasswordDontEqual|]

let updateEmail user newEmail: Result<UserEvent array, DomainError array> =
    if user.Email <> newEmail then
        Ok [|UserEvent.EmailUpdated (user.Id, newEmail)|]
    else
        Ok [||]
    
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
            