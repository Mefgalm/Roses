module Handle.CoreError

open Kernel
open Read
open Write

[<RequireQualifiedAccess>]
type CoreError =
    | Domain of DomainError
    | Read of ReadError
    | Write of WriteError


let private errorToCore f result = 
    match result with
    | Ok x -> Ok x
    | Error domainErrors -> Error (domainErrors |> Array.map f)

let domainToCore result = errorToCore CoreError.Domain result

let writeToCore result = errorToCore CoreError.Write result
 
let readToCore result = errorToCore CoreError.Read result