module SourceLink.Pdb

open System
open System.IO
open System.Collections.Generic
open SourceLink.Pe
open SourceLink.Extension
open SourceLink.Exception

let computeChecksums files =
    use md5 = Security.Cryptography.MD5.Create()
    let computeHash file =
        use fs = File.OpenRead file
        md5.ComputeHash fs
    let checksums = Dictionary<string, string>()
    for f in files do
        checksums.[f |> computeHash |> Hex.encode] <- f
    checksums

let readLines (bytes:byte[]) =
    use sr = new StreamReader(new MemoryStream(bytes))
    seq {
        while not sr.EndOfStream do
            yield sr.ReadLine()
    }
    |> Seq.toArray

type PdbInfo() =
    member val File = String.Empty with set, get
    member val Guid = defaultGuid with set, get
    member val Age = 0 with set, get
    member val StreamCount = 0 with set, get
    member val StreamNames = SortedDictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,int> with set, get
    member val Filenames = SortedDictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,string> with set, get
    member val Checksums = Dictionary(StringComparer.OrdinalIgnoreCase) :> IDictionary<string,string> with set, get
    member val PeAge = 0u with set, get
    member x.PeId with get() = x.Guid.ToStringN + x.PeAge.ToString()
    member val SrcSrv = [||] with set, get

type PdbStream() =
    member val Index = 0 with set, get
    member val ByteCount = 0 with set, get
    member val Pages = Array.create 0 0 with set, get

type PdbFile(file) =
    let pi = PdbInfo()
    do pi.File <- file
    let br = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))

    do // check header
        let c00 = char 0x00 // null \0 null character
        let c0D = char 0x0D // cr \r carriage return
        let c0A = char 0x0A // lf \n line feed
        let c1A = char 0x1A // ctrl+z substitute character
        let msf = sprintf "Microsoft C/C++ MSF 7.00%c%c%cDS%c%c%c" c0D c0A c1A c00 c00 c00
        if false = msf.ToUtf8.CollectionEquals (br.ReadBytes 32) then
            failwithf "pdb header didn't match"
    
    // read rest of header
    let pageByteCount = br.ReadInt32()
    do br.Skip 4 // free pages
    let usedPageCount = br.ReadInt32()
    let directoryByteCount = br.ReadInt32()
    do br.Skip 4
    let directoryPageNumber = br.ReadInt32()

    // reading functions
    let countPages nBytes = (nBytes + pageByteCount - 1) / pageByteCount
    let goToPage n = br.Position <- n * pageByteCount
    let readPage (bytes:byte[]) page offset count =
        goToPage page
        let read = br.Read(bytes, offset, count)
        if read <> count then failwithf "tried reading %d bytes at offset %d, but only read %d" count offset read
    let readStreamBytes (stream:PdbStream) =
        let pages = stream.Pages
        let bytes = Array.create stream.ByteCount 0uy
        for i in 0 .. pages.Length - 2 do
            readPage bytes pages.[i] (i * pageByteCount) pageByteCount
        let i = pages.Length - 1
        readPage bytes pages.[i] (i * pageByteCount) (stream.ByteCount - (i * pageByteCount))
        bytes
    let readStream stream = new MemoryStream(readStreamBytes stream)
    let streamReader stream = new BinaryReader(readStream stream)

    // read stream directory pointers
    let directoryStream = PdbStream()
    do 
        directoryStream.ByteCount <- directoryByteCount
        directoryStream.Pages <- Array.create (countPages directoryByteCount) 0
        goToPage directoryPageNumber
        for i in 0 .. directoryStream.Pages.Length - 1 do
            directoryStream.Pages.[i] <- br.ReadInt32()

    // read stream directory
    let brDirectory = streamReader directoryStream
    do pi.StreamCount <- brDirectory.ReadInt32()
    let streams = Array.create pi.StreamCount (PdbStream())
    do 
        for i in 0 .. pi.StreamCount - 1 do
            let stream = PdbStream()
            streams.[i] <- stream
            let byteCount = brDirectory.ReadInt32()
            stream.ByteCount <- byteCount
            let pageCount = countPages byteCount
            stream.Pages <- Array.create pageCount 0
        for i in 0 .. pi.StreamCount - 1 do
            for j in 0 .. streams.[i].Pages.Length - 1 do
                streams.[i].Pages.[j] <- brDirectory.ReadInt32()
    
    do // read stream 1, names
        use br = streamReader streams.[1]
        let version = br.ReadInt32() // of stream
        let signature = br.ReadInt32()
        pi.Age <- br.ReadInt32()
        pi.Guid <- br.ReadGuid()
        let namesByteCount = br.ReadInt32()
        let namesByteStart = br.Position // 0x20
        br.Position <- namesByteStart + namesByteCount
        let nameCount = br.ReadInt32()
        let nameCountMax = br.ReadInt32()
        let flags = Array.create (br.ReadInt32()) 0 // bit flags for each nameCountMax
        for i in 0 .. flags.Length - 1 do
            flags.[i] <- br.ReadInt32() 
        let hasName i =
            let a = flags.[i / 32]
            let b = (1 <<< i % 32)
            a &&& b <> 0
        br.Skip 4 // 0
        let positions = List<int*int>(nameCount) // stream name position to stream index
        for i in 0 .. nameCountMax - 1 do
            if hasName i then
                let position = br.ReadInt32()
                let index = br.ReadInt32()
                positions.Add(position, index)
        if positions.Count <> nameCount then
            failwithf "names index count, %d <> %d" positions.Count nameCount
        for position, index in positions do
            br.Position <- namesByteStart + position
            let name = br.ReadCString()
            pi.StreamNames.Add(name, index)

    do // read checksums
        let prefix = "/src/files/"
        pi.StreamNames
        |> Seq.map (fun (KeyValue(name, i)) -> name.ToLowerInvariant(), i)
        |> Seq.filter (fun (name, i) -> name.StartsWith prefix)
        |> Seq.iter (fun (name, i) -> 
            let checksum = (readStreamBytes streams.[i]).[72..87] |> Hex.encode
            let filename = name.Substring prefix.Length
            pi.Filenames.[filename] <- checksum
            pi.Checksums.[checksum] <- filename
        )

    member x.Dispose() =
        use br = br
        use brDirectory = brDirectory
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

    member x.ReadStreamBytes i = readStreamBytes streams.[i]
    member x.Info with get() = pi
    member x.HasSrcSrv = x.Info.StreamNames.ContainsKey "SRCSRV"
    member x.ReadSrcSrv = if x.HasSrcSrv then x.ReadStreamBytes x.Info.StreamNames.["SRCSRV"] |> readLines else [||]
