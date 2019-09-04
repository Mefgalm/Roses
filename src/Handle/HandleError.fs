module Handle.HandleError

[<RequireQualifiedAccess>]
type DomainError =
    | WrongStatus
    | PassworAndRepeatPasswordDontEqual
    | WrongPasswordPattern
    | WrongEmailPattern

