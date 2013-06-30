module SourceLink.Pdb

open System
open System.IO
open System.Collections.Generic
open SourceLink.Pe
open SourceLink.Extension
open SourceLink.Exception
open System.Collections

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

type PdbStream() =
    member val Index = 0 with set, get
    member val ByteCount = 0 with set, get
    member val Pages = Array.create 0 0 with set, get

type PdbFile(file) =
    let pi = PdbInfo()
    do pi.File <- file
    let fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)
    let br = new BinaryReader(fs)

    let skip (i:int) = fs.Position <- fs.Position + int64 i
    
    let checkHeader =
        let c00 = char 0x00 // null \0 null character
        let c0D = char 0x0D // cr \r carriage return
        let c0A = char 0x0A // lf \n line feed
        let c1A = char 0x1A // ctrl+z substitute character
        let msf = sprintf "Microsoft C/C++ MSF 7.00%c%c%cDS%c%c%c" c0D c0A c1A c00 c00 c00
        if false = msf.ToUtf8.CollectionEquals (br.ReadBytes 32) then
            failwithf "pdb header didn't match"
    
    // read rest of header
    let pageByteCount = br.ReadInt32()
    do skip 4 // free pages
    let usedPageCount = br.ReadInt32()
    let directoryByteCount = br.ReadInt32()
    do skip 4
    let directoryPageNumber = br.ReadInt32()

    // reading functions
    let countPages nBytes = (nBytes + pageByteCount - 1) / pageByteCount
    let goToPage n = fs.Position <- n * pageByteCount |> int64
    let readPage bytes page offset count =
        goToPage page
        let read = fs.Read(bytes, offset, count)
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
    do directoryStream.ByteCount <- directoryByteCount
    do directoryStream.Pages <- Array.create (countPages directoryByteCount) 0
    do goToPage directoryPageNumber
    do for i in 0 .. directoryStream.Pages.Length - 1 do
        directoryStream.Pages.[i] <- br.ReadInt32()

    // read stream directory
    let brDirectory = streamReader directoryStream
    let streamCount = brDirectory.ReadInt32()
    let streams = Array.create streamCount (PdbStream())
    do for i in 0 .. streamCount - 1 do
        let stream = PdbStream()
        streams.[i] <- stream
        let byteCount = brDirectory.ReadInt32()
        stream.ByteCount <- byteCount
        let pageCount = countPages byteCount
        stream.Pages <- Array.create pageCount 0
    do for i in 0 .. streamCount - 1 do
        for j in 0 .. streams.[i].Pages.Length - 1 do
            streams.[i].Pages.[j] <- brDirectory.ReadInt32()

    // TODO check file length
    // TODO check # of pages in directory
    // TODO read stream 1
    //
    do printfn "done reading" // temporary


    member x.Dispose() =
        use fs = fs
        use br = br
        use brDirectory = brDirectory
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

let readLines (bytes:byte[]) =
    use sr = new StreamReader(new MemoryStream(bytes))
    seq {
        while not sr.EndOfStream do
            yield sr.ReadLine()
    }
    |> Seq.toArray
