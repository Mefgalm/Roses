module Kernel.Domain.DomainTypes

open System
open Kernel

let private baseExample value exampleFn =
    match exampleFn value with
    | Ok example -> example
    | Error domErrors -> failwith (sprintf "%A" domErrors)


type Email = Email of string
    with 
        static member Create str =
            if str <> ""
            then Ok (Email str)
            else Error [|DomainError.WrongEmailPattern|]
        static member Example = baseExample "mefalm@gmail.com" Email.Create
        static member Get (Email str) = str


type Password = Password of string
    with 
        static member Create str =
            if str <> ""
            then Ok (Password str)
            else Error [|DomainError.WrongPasswordPattern|]
        static member Example = baseExample "123456" Password.Create
        static member Get (Password str) = str
    
    
type CreatedDate = CreatedDate of DateTime
    with 
        static member Create date =
            Ok (CreatedDate date)
        static member Example = baseExample DateTime.UtcNow CreatedDate.Create
        static member Get (CreatedDate date) = date