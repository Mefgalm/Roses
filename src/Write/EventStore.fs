module Write.EventStore

open System
open System
open System.IO
open EventStore.ClientAPI
open MBrace.FsPickler.Json
open FSharp.Control.Tasks


[<Literal>]
let eventStoreConnectionStrings = "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"

let writeEvent (stream: Guid) eventType version events =
    Async.AwaitTask <| task {
    try 
        let connection = EventStoreConnection.Create(eventStoreConnectionStrings)
        
        do! connection.ConnectAsync()
        
        let jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)
        
        use msStream = new MemoryStream()
        jsonSerializer.Serialize(msStream, events)            
        
        let esEvent = new EventData(Guid.NewGuid(), eventType, true, msStream.ToArray(), Array.empty)
        
        printfn "version %i" version
        
        let! _ = connection.AppendToStreamAsync(stream.ToString(), version, esEvent)
        
        return Ok ()
    with
    | e ->
        return Error e.Message 
}
    
    
let readEvents stream =
    Async.AwaitTask <| task {
    try 
        let connection = EventStoreConnection.Create eventStoreConnectionStrings
        
        do! connection.ConnectAsync()
        
        let mutable isEnd = false
        let mutable start = 0L
        let count = 100
        
        let events = new System.Collections.Generic.List<ResolvedEvent>()        
        
        while not isEnd do                    
            let! eventsSlice = connection.ReadStreamEventsForwardAsync(stream, start, count, false)
            
            events.AddRange eventsSlice.Events        
            
            start <- start + (count |> int64)
            isEnd <- eventsSlice.IsEndOfStream
            
        return Ok (events.ToArray())
    with
    | e ->
        return Error e.Message
}

