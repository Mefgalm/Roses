namespace Read

[<RequireQualifiedAccess>]
type ReadError =
    | Exeption of string
    | SomeError

