namespace SourceLink

open System
open System.Collections.Generic
open Pe

type PdbStream() =
    member val Index = 0 with set, get
    member val ByteCount = 0 with set, get
    member val Pages = Array.create 0 0 with set, get

type RootStream() =
    member val Streams = Array.zeroCreate<PdbStream> 0 with set, get

type PdbName() =
    member val Index = 0 with set, get
    member val Stream = 0 with set, get
    member val Name = String.Empty with set, get

type PdbInfo() =
    member val Version = 0 with set, get
    member val Signature = 0 with set, get
    member val Guid = defaultGuid with set, get
    member val Age = 0 with set, get
    member val NameIndexMax = 0 with set, get
    member val Names = List<PdbName>() with get
    member val StreamToPdbName = SortedDictionary() :> IDictionary<int,PdbName> with get
    member val NameToPdbName = SortedDictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,PdbName> with get
    member x.AddName (name:PdbName) =
        x.Names.Add name
        x.StreamToPdbName.Add(name.Stream, name)
        x.NameToPdbName.Add(name.Name, name)
    member val SrcSrv = Array.zeroCreate<string> 0 with set, get
    member val Tail = Array.create 0 0uy with set, get // TODO unknown bytes, sometimes 4 or 8

