module Kernel.Domain.User

open System
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes
open Newtonsoft.Json
open Kernel


type Role =
    | SuperAdmin

[<RequireQualifiedAccessAttribute>]
type UserEvent = 
    | UserCreated of Id: Guid * Email * Password * CreatedDate * Roles: Role list
    | EmailUpdated of Id: Guid * NewEmail: Email


type User =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: CreatedDate
      Roles: Role list }
    with
        static member Default =
            { Id = Guid.Empty
              Email = Email.Example
              Password = Password.Example
              CreatedDate = CreatedDate.Example
              Roles = []}


let createUser id email password repeatPassword roles createdDate =
    if password = repeatPassword then
        Ok [|UserEvent.UserCreated (id, email, password, createdDate, roles)|]
    else
        Error [|DomainError.PassworAndRepeatPasswordDontEqual|]


let signIn user (email: string) (password: string) =
    if (user.Email |> Email.Get) = email && 
       (user.Password |> Password.Get) = password then
        Ok [||]
    else
        Error [|DomainError.PasswordOrEmailIsWrong|]


let updateEmail user newEmail: Result<UserEvent array, DomainError array> =
    if user.Email <> newEmail then
        Ok [|UserEvent.EmailUpdated (user.Id, newEmail)|]
    else
        Ok [||]

    
let applyEvent user event =
    match event with
    | UserEvent.UserCreated (id, email, password, createdDate, roles) ->
        { Id = id
          Email = email
          Password = password
          Roles = roles
          CreatedDate = createdDate } //TODO try re-write to copy-with operator
        
    | UserEvent.EmailUpdated (_, newEmail) ->
        { user with Email = newEmail }
        
        
let applyEvents user events =
    events |> Seq.fold applyEvent user
            