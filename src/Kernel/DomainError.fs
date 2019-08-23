namespace Kernel

[<RequireQualifiedAccess>]
type DomainError =
    | WrongStatus
    | PassworAndRepeatPasswordDontEqual
    | WrongPasswordPattern
    | WrongEmailPattern

