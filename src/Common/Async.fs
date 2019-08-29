module Common.Async

let Map f x =
    async.Bind(x, fun a -> async.Return (f a))

