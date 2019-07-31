module Kernel.Domain.DomainTypes

let private baseExample value exampleFn =
    match exampleFn value with
    | Ok example -> example
    | Error errorMsg -> failwith errorMsg

type Email = private Email of string
module Email =
    let create str =
        if str <> ""
        then Ok (Email str)
        else Error "email must not be empty"
        
    let example = baseExample "mefalm@gmail.com" create 


type Password = private Password of string
module Password =
    
    let create str =
        if str <> ""
        then Ok (Password str)
        else Error "password must not be empty"
        
    let example = baseExample "123456" create        
            
    let get (Password str) = str    