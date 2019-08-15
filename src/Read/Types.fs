module Read.Types

open System


type IReadEntity =
    abstract member Id: string
    
//[<CLIMutable>]
type UserRead =
    { Id: string
      Email: string
      Password: string
      CreatedDate: DateTime }
    
    interface IReadEntity with
        member this.Id = this.Id
        
        
type SuperAdminRead =
    { Id: string
      Email: string
      Password: string
      CreatedDate: DateTime }
    
    interface IReadEntity with
        member this.Id = this.Id