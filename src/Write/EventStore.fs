module Write.EventStore

open System
open System
open System.IO
open System.IO
open EventStore.ClientAPI
open MBrace.FsPickler.Json
open FSharp.Control.Tasks
open Common.AsyncResult
open Common.JsonConv


[<Literal>]
let eventStoreConnectionStrings = "ConnectTo=tcp://admin:changeit@localhost:1113; HeartBeatTimeout=500"

let writeEvent<'t> (stream: Guid) version events =
    Async.AwaitTask <| task {
    try 
        let connection = EventStoreConnection.Create(eventStoreConnectionStrings)
        
        do! connection.ConnectAsync()
        
        let eventType = typedefof<'t>.Name
        
        let serializedEvent = Common.JsonConv.serialize events

        let esEvent = new EventData(Guid.NewGuid(), eventType, true, Text.Encoding.UTF8.GetBytes serializedEvent, Array.empty)
        
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


//let readDomainEvents stream = asyncResult {
//    let! events = readEvents stream
    
//    let jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)        
    
//    let answer = events |> Array.map(fun x ->
//        use msStream = new MemoryStream()
        
//        let result = Convert.ChangeType(jsonSerializer.Deserialize(msStream), Type.GetType x.Event.EventType)
    
//        result)    
    
//    return answer   
//}
    