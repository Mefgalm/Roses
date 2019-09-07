module Read.ReadDb


open System
open MongoDB.Bson
open MongoDB.Driver
open Read.Types
open FSharp.Control.Tasks
open MongoDB.Driver
open Types
open System.Linq.Expressions
open System.Threading.Tasks

[<Literal>]
let userColl = "users"

[<Literal>]
let superAdminColl = "superAdmin"

let client = MongoClient(new MongoUrl("mongodb://localhost:27017"))

let database = client.GetDatabase("test")

let safeCall exnGenerator f = task {
    try        
        return! f ()
    with e ->
        return Error [|ReadError.Exception (exnGenerator e)|]
}


let private getCollectionName (entity: 'a when 'a :> IReadEntity) =
    match box entity with
    | :? UserRead -> userColl
    | :? SuperAdminRead -> superAdminColl
    | _ -> failwith "(getCollectionName) Cannot find type"
    

let addEntity<'a when 'a :> IReadEntity> (entity: 'a) =
    safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            do! database.GetCollection<'a>(entity |> getCollectionName).InsertOneAsync entity
            return Ok ()
        }) 
    |> Async.AwaitTask
    
    
let updateEntity<'a when 'a :> IReadEntity> (entity: 'a) =
    safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            let! replaceResult = database.GetCollection<'a>(entity |> getCollectionName).ReplaceOneAsync((fun x -> x.Id = entity.Id), entity)
            
            if replaceResult.IsAcknowledged then
                return Ok ()
            else
                return (Error [|ReadError.SomeError|])
        })
    |> Async.AwaitTask
    
    
let getEntity<'a when 'a :> IReadEntity> collection id =
    safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            let! cursor = database.GetCollection<'a>(collection).FindAsync(fun x -> x.Id = id)
            let! single = cursor.SingleAsync()
            return Ok single
        })
    |> Async.AwaitTask        
