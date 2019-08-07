module Read.ReadDb


open System
open MongoDB.Bson
open MongoDB.Driver
open Read.Types
open FSharp.Control.Tasks
open MongoDB.Driver
open System.Linq.Expressions

[<Literal>]
let usersCol = "users"

let client = MongoClient(new MongoUrl("mongodb://localhost:27017"))

let database = client.GetDatabase("test")

let addUser id email password createdDate =
    Async.AwaitTask <| task {
    let userRead = { Id = id
                     Email = email
                     Password = password
                     CreateDate = createdDate }    
    
    do! database.GetCollection<UserRead>(usersCol).InsertOneAsync(userRead)
            
    return Ok ()            
}

let getUser id =
    Async.AwaitTask <| task {
    let! cursor = database.GetCollection<UserRead>(usersCol).FindAsync(fun x -> x.Id = id)        
        
    let! single = cursor.SingleAsync()         
        
    return Ok single         
}


let updateUser id newUser =
    Async.AwaitTask <| task {
        
    let! result = database.GetCollection<UserRead>(usersCol).ReplaceOneAsync((fun x -> x.Id = id), newUser)       
       
    if result.IsAcknowledged then               
        return Ok ()
    else
        return Error "error"
}

