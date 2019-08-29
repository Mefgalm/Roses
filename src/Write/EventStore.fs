module Write.EventStore

open System
open System.Collections.Generic
open EventStore.ClientAPI
open FSharp.Control.Tasks

[<Literal>]
let eventStoreConnectionStrings = "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"

let private toBytes:(string -> byte[]) = Text.Encoding.UTF8.GetBytes
let private toString:(byte[] -> string) = Text.Encoding.UTF8.GetString

let private safeCall exnGenerator f = task {
    try        
        return! f()
    with e ->
        return Error [|WriteError.Exception (exnGenerator e)|]
}

let writeEvent<'t> (stream: Guid) version events =
    safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            let connection = EventStoreConnection.Create(eventStoreConnectionStrings)
            
            do! connection.ConnectAsync()
            
            let eventType = typedefof<'t>.Name
            
            let serializedEvent = Common.JsonConv.serialize events

            let esEvent = new EventData(Guid.NewGuid(), eventType, true, serializedEvent |> toBytes, Array.empty)
            
            let! _ = connection.AppendToStreamAsync(stream.ToString(), version, esEvent)                
            
            return Ok ()
        })
    |> Async.AwaitTask

        
let readEventsSize takeSize stream =
    safeCall
        (fun exn -> exn.Message)
        (fun () -> task {
            let connection = EventStoreConnection.Create eventStoreConnectionStrings
            
            do! connection.ConnectAsync()
            
            let mutable isEnd = false
            let mutable start = 0L
            
            let events = List<(int64 * string)>()        
            
            while not isEnd do                    
                let! eventsSlice = connection.ReadStreamEventsForwardAsync(stream, start, takeSize, false)
                
                events.AddRange (eventsSlice.Events |> Array.map(fun e -> e.Event.EventNumber, e.Event.Data |> toString))        
                
                start <- start + (takeSize |> int64)
                isEnd <- eventsSlice.IsEndOfStream
                
            return Ok (events.ToArray())
        })
    |> Async.AwaitTask

let readEvents = readEventsSize 500