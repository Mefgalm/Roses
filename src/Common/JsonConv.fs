module Common.JsonConv

open System.IO
open MBrace.FsPickler.Json
open Newtonsoft.Json
open Newtonsoft.Json.Converters



let serialize obj =
    //let jsonSerializer = FsPickler.CreateJsonSerializer(indent = false)
        
    //use msStream = new MemoryStream()
    //jsonSerializer.Serialize(msStream, obj)
    
    //msStream.ToArray()

    let jsonConverters = new System.Collections.Generic.List<JsonConverter>()
    jsonConverters.Add(DiscriminatedUnionConverter())

    JsonConvert.SerializeObject(obj, JsonSerializerSettings(Converters = jsonConverters))
    
let deserialize<'a> text =
    let testType = typedefof<'a>

    printfn "type is %A" testType

    JsonConvert.DeserializeObject(text, typedefof<'a>) :?> 'a
