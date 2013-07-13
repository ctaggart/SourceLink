namespace SourceLink

open System
open System.Collections.Generic
open Pe

type PdbStream() =
    member val ByteCount = 0 with set, get
    member val Pages = Array.create 0 0 with set, get

// data in the root stream
type PdbRoot() =
    member val Streams = List<PdbStream>() with set, get
    member x.AddStream (pdbStream:PdbStream) =
        x.Streams.Add pdbStream
        x.Streams.Count - 1

type PdbName() =
    member val Index = 0 with set, get
    member val Stream = 0 with set, get
    member val Name = String.Empty with set, get

// data in the info stream
type PdbInfo() =
    member val Version = 0 with set, get
    member val Signature = 0 with set, get
    member val Guid = defaultGuid with set, get
    member val Age = 0 with set, get
    member val NameIndexMax = 0 with set, get
    member val Names = List<PdbName>() with get // will be ordered by index
    member val StreamToPdbName = SortedDictionary() :> IDictionary<int,PdbName> with get
    member val NameToPdbName = SortedDictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,PdbName> with get
    member x.AddName (name:PdbName) =
        x.Names.Add name
        x.StreamToPdbName.Add(name.Stream, name)
        x.NameToPdbName.Add(name.Name, name)
    member x.AddNewName name =
        let pdbName = PdbName()
        pdbName.Name <- name
        let lastIndex = x.Names.[x.Names.Count-1].Index
        pdbName.Index <- lastIndex + 1
        if pdbName.Index >= x.NameIndexMax then
            x.NameIndexMax <- pdbName.Index + 1 // TODO must it be greater? Increments? throw instead?
        let streamNumbers = x.StreamToPdbName.Keys |> List.ofSeq
        let lastStream = streamNumbers.[streamNumbers.Length-1]
        pdbName.Stream <- lastStream + 1
        x.AddName pdbName
        pdbName
    member val SrcSrv = Array.zeroCreate<string> 0 with set, get
    member val Tail = Array.create 0 0uy with set, get // TODO unknown bytes, sometimes 4 or 8

