module Read.UserReadDb

open Types
open FSharp.Control.Tasks
open MongoDB.Driver

let getUser id =
    ReadDb.getEntity<UserRead> ReadDb.userColl id

let getUserByEmail email =
    ReadDb.safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            let! cursor = ReadDb.database.GetCollection<UserRead>(ReadDb.userColl).FindAsync<UserRead>(fun x -> x.Email = email)
            let! single = cursor.SingleAsync()
            return Ok single
        })
        |> Async.AwaitTask