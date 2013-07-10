namespace SourceLink

open System
open System.IO
open System.Collections.Generic
open SourceLink.Pe
open SourceLink.Extension
open SourceLink.Exception
open SourceLink.File

type PdbFile(file) =
    
    let fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
    let br = new BinaryReader(fs)
//    let bw = new BinaryWriter(fs)

    do // check header
        let c00 = char 0x00 // null \0 null character
        let c0D = char 0x0D // cr \r carriage return
        let c0A = char 0x0A // lf \n line feed
        let c1A = char 0x1A // ctrl+z substitute character
        let msf = sprintf "Microsoft C/C++ MSF 7.00%c%c%cDS%c%c%c" c0D c0A c1A c00 c00 c00
        if false = msf.ToUtf8.CollectionEquals (br.ReadBytes 32) then
            failwithf "pdb header didn't match"
    
    // read rest of header
    let pageByteCount = br.ReadInt32() // 0x20
    let pagesFree = br.ReadInt32() // 0x24 TODO not sure meaning, often 2 free pages every 0x200 pages: 0x201 0x202 0x401 0x402 ...
    let pageCount = br.ReadInt32() // 0x28 for file
    let rootByteCount = br.ReadInt32() // 0x2C
    do br.Skip 4 // 0
    let rootPage = br.ReadInt32() // 0x34

    do // check pdb
        let length = int br.BaseStream.Length
        do if length % pageByteCount <> 0 then
            failwithf "pdb length %% bytes per page <> 0, %d, %d" length pageByteCount
        if length / pageByteCount <> pageCount then
            failwithf "pdb length does not match page count, length: %d, bytes per page: %d, page count: %d" length pageByteCount pageCount

    // reading functions
    let countPages nBytes = (nBytes + pageByteCount - 1) / pageByteCount
    let goToPage n = br.Position <- n * pageByteCount
    let goToEnd() = fs.Seek(0L, SeekOrigin.End) |> ignore
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

    // read root stream
    let readRoot streamRoot =
        let root = PdbRoot()
        use brDirectory = streamReader streamRoot
        let streamCount = brDirectory.ReadInt32()
        if streamCount <> 0x0131CA0B then
            let streams = Array.create streamCount (PdbStream())
            for i in 0 .. streamCount - 1 do
                let stream = PdbStream()
                streams.[i] <- stream
                let byteCount = brDirectory.ReadInt32()
                stream.ByteCount <- byteCount
                let pageCount = countPages byteCount
                stream.Pages <- Array.create pageCount 0
            for i in 0 .. streamCount - 1 do
                for j in 0 .. streams.[i].Pages.Length - 1 do
                    let page = brDirectory.ReadInt32()
                    streams.[i].Pages.[j] <- page
            root.Streams <- streams
        root
    let rootPdbStream =
        let pdbStream = PdbStream()
        pdbStream.ByteCount <- rootByteCount
        pdbStream.Pages <- Array.create (countPages rootByteCount) 0
        goToPage rootPage
        for i in 0 .. pdbStream.Pages.Length - 1 do
            pdbStream.Pages.[i] <- br.ReadInt32()
        pdbStream
    let root = readRoot rootPdbStream

    // read info stream
    let info =
        let info = PdbInfo()
        use br = streamReader root.Streams.[1]
        info.Version <- br.ReadInt32() // 0x00 of stream
        info.Signature <- br.ReadInt32() // 0x04
        info.Age <- br.ReadInt32() // 0x08
        info.Guid <- br.ReadGuid() // 0x0C
        let namesByteCount = br.ReadInt32() // 0x16
        let namesByteStart = br.Position // 0x20
        br.Position <- namesByteStart + namesByteCount
        let nameCount = br.ReadInt32()
        info.NameIndexMax <- br.ReadInt32()
        let flagCount = br.ReadInt32()
        let flags = Array.create flagCount 0 // bit flags for each nameCountMax
        for i in 0 .. flags.Length - 1 do
            flags.[i] <- br.ReadInt32() 
        let hasName i =
            let a = flags.[i / 32]
            let b = 1 <<< (i % 32)
            a &&& b <> 0
        br.Skip 4 // 0
        let positions = List<int*PdbName>(nameCount)
        for i in 0 .. info.NameIndexMax - 1 do
            if hasName i then
                let position = br.ReadInt32()
                let name = PdbName()
                name.Index <- i
                name.Stream <- br.ReadInt32()
                positions.Add(position, name)
        if positions.Count <> nameCount then
            failwithf "names count, %d <> %d" positions.Count nameCount
        let tailByteCount = root.Streams.[1].ByteCount - br.Position
        info.Tail <- br.ReadBytes tailByteCount
        for position, name in positions do
            br.Position <- namesByteStart + position
            name.Name <- br.ReadCString()
            info.AddName name
        info

    let freePages = SortedSet<int>()
    let zerosPage = Array.create pageByteCount 0uy

    let allocPages n = 
        let pages = List<int>()
        let freePageList = freePages |> Seq.toList
        if freePageList.Length >= n then
            for i in 0 .. n - 1 do
                let page = freePageList.[i]
                pages.Add page
            freePages.Clear()
        else
            for i in 0 .. freePageList.Length - 1 do
                let page = freePageList.[i]
                pages.Add page
                freePages.Remove page |> ignore
            let newPageCount = n - freePageList.Length
            if newPageCount > 0 then
                goToEnd()
                for i in 0 .. newPageCount do
                    let page = (int fs.Position) / pageByteCount
                    pages.Add page
                    fs.WriteBytes zerosPage
        pages

    member x.Dispose() =
        use fs = fs
        use brr = br
//        use bw = bw
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

    member x.File with get() = file
    member x.ReadPdbStreamBytes pdbStream = readStreamBytes pdbStream
    member x.ReadStreamBytes stream = readStreamBytes root.Streams.[stream]
    member x.Info with get() = info
    member x.HasSrcSrv with get() = info.NameToPdbName.ContainsKey "SRCSRV"
    member x.ReadSrcSrv() = if x.HasSrcSrv then x.ReadStreamBytes info.NameToPdbName.["SRCSRV"].Stream |> readLines else [||]
    member x.RootPage with get() = rootPage
    member x.RootPdbStream with get() = rootPdbStream
    member x.Root with get() = root
    member x.Stream0 with get() = readRoot root.Streams.[0]
    member x.PagesFree with get() = pagesFree
    member x.PageCount with get() = pageCount

    member x.FreePages pages =
        for page in pages do
            goToPage page
            fs.WriteBytes zerosPage
            freePages.Add page |> ignore

    member x.FreeStream (i:int) = x.FreePages root.Streams.[i].Pages

    member x.WriteStream (bytes:byte[]) =
        let n = countPages bytes.Length
        let pages = allocPages n
        let j = ref 0
        for i in 0 .. n - 2 do
            goToPage pages.[i]
            fs.Write(bytes, !j * pageByteCount, pageByteCount)
            incr j
        // write last page
        let lastPageBytes = bytes.Length % pageByteCount
        goToPage pages.[n-1]
        if lastPageBytes = 0 then
            fs.Write(bytes, !j * pageByteCount, pageByteCount)
        else
            fs.Write(bytes, !j * pageByteCount, lastPageBytes)
        let pdbStream = PdbStream()
        pdbStream.ByteCount <- bytes.Length
        pdbStream.Pages <- pages.ToArray()
        pdbStream

    member x.WriteRootPage bytes =
        goToPage rootPage
        fs.WriteBytes zerosPage
        goToPage rootPage
        fs.WriteBytes bytes