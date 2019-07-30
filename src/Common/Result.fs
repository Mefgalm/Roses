module Common.Result

let reduceResults results =
    Seq.reduce (fun r1 r2 ->
        match r1, r2 with
        | Ok (), Ok () -> Ok ()
        | Ok (), Error str 
        | Error str, Ok _ -> Error str
        | Error str1, Error str2 -> Error (sprintf "%s %s" str1 str2)) results


type ResultBuilder() =    
    member __.Return(x) = Ok x
    member __.Bind(x, f) = Result.bind f x
    member __.ReturnFrom(x: Result<_, _>) = x
    
    
let result = new ResultBuilder()