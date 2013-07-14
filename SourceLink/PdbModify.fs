module SourceLink.PdbModify

open System.IO
open System.Text
open System.Collections.Generic
open SourceLink
open SourceLink.Extension
open SourceLink.Exception

let createRootPageBytes (pdbStream:PdbStream) =
    use ms = new MemoryStream()
    use bw = new BinaryWriter(ms, Encoding.UTF8, true)
    for page in pdbStream.Pages do
        bw.Write page
    bw.Flush()
    ms.ToArray()

let createRootBytes (root:PdbRoot) =
    use ms = new MemoryStream()
    use bw = new BinaryWriter(ms, Encoding.UTF8, true)
    bw.Write root.Streams.Count
    for stream in root.Streams do
        bw.Write stream.ByteCount
    for stream in root.Streams do
        for page in stream.Pages do
            bw.Write page
    bw.Flush()
    ms.ToArray()

let createInfoBytes (info:PdbInfo) =
    use ms = new MemoryStream()
    use bw = new BinaryWriter(ms, Encoding.UTF8, true)
    bw.Write info.Version
    bw.Write info.Signature
    bw.Write info.Age
    bw.WriteGuid info.Guid
    
    let names = info.StreamToPdbName |> Seq.map (fun (KeyValue(stream, name)) -> name) |> Seq.toArray

    let nameToPosition = Dictionary<string,int>() // could used an array instead
//    let positions = Array.create names.Length (0,0)
//    let i = ref 0
    let position = ref 0
//    let byteCount = ref 0
    let nameBytes = // utf8 bytes without null
        names
        |> Seq.map (fun name ->
            let bytes = name.Name.ToUtf8
            nameToPosition.Add(name.Name, !position)
//            positions.[!i] <- !position, name.Stream
//            byteCount := !byteCount + bytes.Length + 1
            position := !position + bytes.Length + 1
//            incr i
            bytes
            
        )
        |> Seq.toArray

    bw.Write !position
    let lastStream = ref 0
    for bytes in nameBytes do
        bw.Write bytes
        bw.Write 0x0uy // null char

    bw.Write names.Length
    //let nameIndexMax = info.Names.[info.Names.Count-1].Index + 1 // original may be bigger
    let nameIndexMax = info.NameIndexMax
    bw.Write nameIndexMax
    
    let flagCount =
        if nameIndexMax % 32 = 0 then
            nameIndexMax / 32
        else 
            nameIndexMax / 32 + 1
    let flags =
        let flags = Array.create flagCount 0
        for name in info.Names do
            let i = name.Index
            let a = i / 32
            let b = 1 <<< (i % 32)
            flags.[a] <- flags.[a] ||| b
        flags
    
    bw.Write flagCount
    for flag in flags do
        bw.Write flag
    bw.Write 0

    for name in info.Names do
        let position = nameToPosition.[name.Name]
        bw.Write position
        bw.Write name.Stream

    bw.Write info.Tail

    bw.Flush()
    ms.ToArray()