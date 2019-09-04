module Kernel.Domain.SuperAdmin

open System
open Kernel
open Kernel.Domain.DomainTypes

[<RequireQualifiedAccessAttribute>]
type SuperAdminEvent = 
    | Created of Id: Guid * Email * Password * CreatedDate
    | Removed of Id: Guid

type Status =
    | Active
    | Removed

type SuperAdmin =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: CreatedDate
      Status: Status }
    with
        static member Default =
            { Id = Guid.Empty
              Email = Email.Example
              Password = Password.Example
              CreatedDate = CreatedDate.Example
              Status = Status.Active }
            
                        
let create id email password repeatPassword createdDate =
    if password = repeatPassword then
        Ok [|SuperAdminEvent.Created (id, email, password, createdDate)|]
    else
        Error [|DomainError.PassworAndRepeatPasswordDontEqual|]
    
    
let remove superAdmin =
    if superAdmin.Status = Status.Removed then
        Error [|DomainError.WrongStatus|]
    else
       Ok [|SuperAdminEvent.Removed superAdmin.Id|]

let applyEvent superAdmin event =
    match event with
    | SuperAdminEvent.Created (id, email, password, createdDate) ->
        { Id = id
          Email = email
          Password = password
          CreatedDate = createdDate
          Status = Status.Active }
    | SuperAdminEvent.Removed _ ->
        { superAdmin with Status = Status.Removed }
        
        
let applyEvents user events =
    events |> Seq.fold applyEvent user            