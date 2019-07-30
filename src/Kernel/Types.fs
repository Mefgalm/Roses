module Kernel.Types

open System
open Kernel.Domain.User


type Entity<'a> =
    { Version: int64
      Object: 'a }

[<RequireQualifiedAccessAttribute>]
type DomainEvent = 
    | User of UserEvent


[<RequireQualifiedAccessAttribute>]
type Command =
    | CreateUser of UserId: Guid * Email: string * Password: string * RepeatPassword: string    
    | ChangeUserEmail of Entity: Entity<User> * NewEmail: string
    | RemoveUser of UserId: Guid