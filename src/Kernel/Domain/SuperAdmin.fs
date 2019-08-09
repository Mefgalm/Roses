module Kernel.Domain.SuperAdmin

open System
open Kernel.Domain.DomainTypes
open Kernel.Domain.DomainTypes

[<RequireQualifiedAccessAttribute>]
type SuperAdminEvent = 
    | Created of Id: Guid * Email * Password * CreatedDate    


type SuperAdmin =
    { Id: Guid
      Email: Email
      Password: Password
      CreatedDate: CreatedDate }
    with
        static member Default =
            { Id = Guid.Empty
              Email = Email.example
              Password = Password.example
              CreatedDate = CreatedDate.example }
            
                        
let create id email password createdDate =
    Ok [SuperAdminEvent.Created (id, email, password, createdDate)]
    
    
let applyEvent superAdmin event =
    match event with
    | SuperAdminEvent.Created (id, email, password, createdDate) ->
        { Id = id
          Email = email
          Password = password
          CreatedDate = createdDate } //TODO try re-write to copy-with operator
        
        
let applyEvents user events =
    events |> Seq.fold applyEvent user            