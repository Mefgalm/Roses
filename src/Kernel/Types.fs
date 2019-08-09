module Kernel.Types

open Kernel.Domain
open System
open Kernel.Domain.User
open Kernel.Domain.SuperAdmin


type Entity<'a> =
    { Version: int64
      Object: 'a }


[<RequireQualifiedAccessAttribute>]
type DomainTypes =
    | User of User
    | SuperAdmin of SuperAdmin
    

[<RequireQualifiedAccessAttribute>]
type DomainEvent = 
    | User of UserEvent
    | SuperAdmin of SuperAdminEvent


[<RequireQualifiedAccessAttribute>]
type Command =
    | CreateUser of UserId: Guid * Email: string * Password: string * RepeatPassword: string    
    | ChangeUserEmail of Entity: Entity<User> * NewEmail: string
    | RemoveUser of UserId: Guid