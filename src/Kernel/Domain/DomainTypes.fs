module Kernel.Domain.DomainTypes

open System

let private baseExample value exampleFn =
    match exampleFn value with
    | Ok example -> example
    | Error errorMsg -> failwith errorMsg

type Email = Email of string
module Email =
    let create str =
        if str <> ""
        then Ok (Email str)
        else Error "email must not be empty"
        
    let example = baseExample "mefalm@gmail.com" create
    
    let get (Email str) = str


type Password = Password of string
module Password =
    
    let create str =
        if str <> ""
        then Ok (Password str)
        else Error "password must not be empty"
        
    let example = baseExample "123456" create        
            
    let get (Password str) = str
    
    
type CreatedDate = CreatedDate of DateTime
module CreatedDate =
    let create date = Ok (CreatedDate date)
    
    let example = baseExample DateTime.UtcNow create
    
    let get (CreatedDate date) = date
    
    