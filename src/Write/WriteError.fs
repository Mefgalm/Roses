namespace Write

[<RequireQualifiedAccess>]
type WriteError =
    | Exception of Message: string
    | SomeError

