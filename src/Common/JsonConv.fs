module Common.JsonConv

open System.IO
open MBrace.FsPickler.Json

let serialize obj =
    let jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)
        
    use msStream = new MemoryStream()
    jsonSerializer.Serialize(msStream, obj)
    
    msStream.ToArray()
    
let deserialize text textType =
    1