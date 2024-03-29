module Common.Result

type ResultBuilder() =    
    member __.Return(x) = Ok x
    member __.Bind(x, f) = Result.bind f x
    member __.ReturnFrom(x: Result<_, _>) = x
    
let result = new ResultBuilder() 

let reduceResults results =
    Seq.reduce (fun r1 r2 ->
        match r1, r2 with
        | Ok (), Ok () -> Ok ()
        | Ok (), Error str 
        | Error str, Ok _ -> Error str
        | Error str1, Error str2 -> Error (Array.append str1 str2)) results