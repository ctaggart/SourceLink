module SourceLink.Pdb

open System
open System.IO
open System.Collections.Generic

open SourceLink.Pe

let computeChecksums files =
    use md5 = Security.Cryptography.MD5.Create()
    let computeHash file =
        use fs = File.OpenRead file
        md5.ComputeHash fs
    let checksums = Dictionary<string, string>()
    for f in files do
        checksums.[f |> computeHash |> Hex.encode] <- f
    checksums

type PdbInfo() =
    member val File = String.Empty with set, get
    member val Guid = defaultGuid with set, get
    member val Age = 0 with set, get
    member val StreamNames = Dictionary() :> IDictionary<string,int> with set, get
    member val Checksums = SortedDictionary() :> IDictionary<string,string> with set, get
    member val PeAge = 0u with set, get
    member x.PeId with get() = x.Guid.ToStringN + x.PeAge.ToString()
    member val SrcSrv = [||] with set, get

type PdbStream(file) =
    let pi = PdbInfo()
    do
        pi.File <- file
    let fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)

    // TODO finish reading it

    member x.Dispose() =
        use fs = fs
        GC.SuppressFinalize()
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()


let readLines (bytes:byte[]) =
    use sr = new StreamReader(new MemoryStream(bytes))
    seq {
        while not sr.EndOfStream do
            yield sr.ReadLine()
    }
    |> Seq.toArray
