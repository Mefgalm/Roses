module Kernel.Types

open Kernel.Domain
open System
open Kernel.Domain.User

type Entity<'a> =
    { Version: int64
      Object: 'a }


[<RequireQualifiedAccessAttribute>]
type DomainTypes =
    | User of User
    

[<RequireQualifiedAccessAttribute>]
type DomainEvent = 
    | User of UserEvent


[<RequireQualifiedAccessAttribute>]
type Command =
    | CreateUser of UserId: Guid * Email: string * Password: string * RepeatPassword: string * Roles : Role list
    | ChangeUserEmail of Entity: Entity<User> * NewEmail: string
    | RemoveUser of UserId: Guid
    | SignIn of User: User * Email: string * Password: string
    | DoNothing
