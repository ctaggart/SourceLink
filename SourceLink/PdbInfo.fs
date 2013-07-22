namespace SourceLink

open System
open System.Collections.Generic

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
    member val Stream = 0 with set, get
    member val Name = String.Empty with set, get
    member val FlagIndex = 0 with set, get

// data in the info stream
type PdbInfo() =
    member val Version = 0 with set, get
    member val Signature = 0 with set, get
    member val Guid = Guid() with set, get
    member val Age = 0 with set, get
    member val FlagIndexMax = 0 with set, get
    member val FlagCount = 0 with set, get
    member val StreamToPdbName = SortedDictionary() :> IDictionary<int,PdbName> with get
    member val NameToPdbName = SortedDictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,PdbName> with get
    member val FlagIndexToPdbName = SortedDictionary() :> IDictionary<int,PdbName> with get
    member val FlagIndexes = SortedSet() with get
    member x.ClearFlags() =
        x.FlagIndexes.Clear()
        x.FlagIndexToPdbName.Clear()
    member x.AddFlag (name:PdbName) =
        x.FlagIndexes.Add(name.FlagIndex) |> ignore
        x.FlagIndexToPdbName.Add(name.FlagIndex, name)
    member x.AddName (name:PdbName) =
        x.StreamToPdbName.Add(name.Stream, name)
        x.NameToPdbName.Add(name.Name, name)
        x.AddFlag name
//    member internal x.GetFlagIndex() =
//        seq { 1 .. x.FlagIndexMax - 1 } // start at 0? 1?
//        |> Seq.find (fun i -> false = x.FlagIndexes.Contains i)
    member x.AddNewName name =
        let pdbName = PdbName()
        pdbName.Name <- name
        let streamNumbers = x.StreamToPdbName.Keys |> List.ofSeq
        let lastStream = streamNumbers.[streamNumbers.Length-1]
        pdbName.Stream <- lastStream + 1
//        pdbName.FlagIndex <- x.GetFlagIndex()
        x.AddName pdbName
        pdbName
    member val SrcSrv = Array.zeroCreate<string> 0 with set, get
    member val Tail = Array.create 0 0uy with set, get // TODO unknown bytes, sometimes 4 or 8

