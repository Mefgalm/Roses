[<AutoOpen>]
module Common.AsyncResult

let asyncMap f x = async.Bind(x, async.Return << f)

let liftAsync x = 
    async { return x }

let pureValue (value: 'a) : Async<Result<'a, 'b>> = 
    async { return Ok value }

let returnFrom (value: Async<Result<'a, 'b>>) : Async<Result<'a, 'b>> = 
    value

let bind (binder: 'a -> Async<Result<'b, 'c>>) (asyncResult: Async<Result<'a, 'c>>) : Async<Result<'b, 'c>> = 
    async {
        let! result = asyncResult
        match result with
        | Ok x -> return! binder x
        | Error x -> return! Error x |> liftAsync
    }
    
let returnResult (resultValue: Result<'a, 'b>): Async<Result<'a, 'b>> =
    async { return resultValue }

let bindResult (binder: 'a -> Async<Result<'b, 'c>>) (result: Result<'a, 'c>) : Async<Result<'b, 'c>> = 
    bind binder (liftAsync result)

let bindAsync (binder: 'a -> Async<Result<'b, 'c>>) (asnc: Async<'a>) : Async<Result<'b, 'c>> = 
    bind binder (asyncMap Ok asnc)

type AsyncResultBuilder() = 
    member __.Return value = pureValue value
    member __.ReturnFrom value = returnFrom value
    member __.ReturnFrom resultValue = returnResult resultValue
    member __.TryWith(expr, handler) = try expr with e -> handler e
    member __.Bind(result, binder) = bindResult binder result
    member __.Bind(asyncResult, binder) = bind binder asyncResult

let asyncResult = AsyncResultBuilder()