﻿namespace Read

[<RequireQualifiedAccess>]
type ReadError =
    | Exception of Message: string
    | SomeError

