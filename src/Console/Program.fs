// Learn more about F# at http://fsharp.org

open Write
open EventStore.ClientAPI
open Handle
open System
open Handle.JWTService
open Kernel.Domain.DomainTypes
    
//let getRose stream =
//    desEvents<Domain.Rose.RoseEvent> (fun evs -> Domain.User.applyEvents Domain.User.User.Default evs) stream

//let restoreUser events =
//    Domain.User.applyEvents Domain.User.User.Default events


type TestA = { Id : string }
type TestB = { Id : string; Name: string }


[<EntryPoint>]
let main argv =
    let createUserRequest = RequestHandler.Request.CreateSuperAdminUser ("mefgalm2@gmail.com", "123", "123")
    
    let secret = Secret.Create "1234567890qwerty"
    let audience = Audience.Create "Test"
    let issuer = Issuer.Create "Test"

    let daysToExpire = DaysToExpire.Create 10.
    let id = Guid.NewGuid()
    let email = 
        match Email.Create "mefgalm@gmail.com" with 
        | Ok x -> x 
        | Error _ -> failwith "Email"
    let roles = ["SuperAdmin"; "Admin"]
    let nowTime = DateTime.Now

    let token = JWTService.generate secret issuer audience daysToExpire nowTime { Id = id; Email = email; Roles = roles }

    printfn "Token is %s" token

    let isTokenValid = JWTService.validateToken secret issuer audience token

    printfn "Is Valid Token = %b" isTokenValid

    let tokenPayload = JWTService.getPayload secret issuer audience token

    printfn "Token payload is %A" tokenPayload

    //JWTService.

    //match RequestHandler.requestHandler createUserRequest |> Async.RunSynchronously with
    //| Ok result -> sprintf "%A" result
    //| Error error -> sprintf "%A" error
    //|> printfn "result is -> %s"

    0