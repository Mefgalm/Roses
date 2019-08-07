module Read.Types

open System

//[<CLIMutable>]
type UserRead =
    { Id: string
      Email: string
      Password: string
      CreateDate: DateTime }