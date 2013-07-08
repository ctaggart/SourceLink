module SourceLink.PdbModify

open System.IO
open System.Text
open System.Collections.Generic
open SourceLink
open SourceLink.Extension
open SourceLink.Exception

//let createRootBytes (info:PdbInfo) = // TODO stream and page

let createInfoBytes (info:PdbInfo) =
    use ms = new MemoryStream()
    use bw = new BinaryWriter(ms)
    bw.Write info.Version
    bw.Write info.Signature
    bw.Write info.Age
    bw.WriteGuid info.Guid
    
    let names = info.StreamToPdbName |> Seq.map (fun (KeyValue(stream, name)) -> name) |> Seq.toArray

    let nameToPosition = Dictionary<string,int>()
    let position = ref 0
    let nameBytes = // utf8 bytes without null
        names
        |> Seq.map (fun name ->
            let bytes = name.Name.ToUtf8
            nameToPosition.Add(name.Name, !position)
            position := !position + bytes.Length + 1
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
        bw.Write nameToPosition.[name.Name]
        bw.Write name.Stream

    bw.Write info.Tail

    ms.ToArray()