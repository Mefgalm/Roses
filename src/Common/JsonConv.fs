module Common.JsonConv

open FSharp.Json

let serialize obj =
    Json.serialize obj
    
let deserialize<'a> text =    
    Json.deserialize<'a> text