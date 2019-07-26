module Kernel.Types

open System
open Kernel.Domain.User

[<RequireQualifiedAccessAttribute>]
type DomainEvent = 
    | User of UserEvent


[<RequireQualifiedAccessAttribute>]
type Command =
    | CreateUser of UserId: Guid * Email: string * Password: string * RepeatPassword: string    
    | ChangeUserEmail of UserId: Guid * NewEmail: string
    | RemoveUser of UserId: Guid