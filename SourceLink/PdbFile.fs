namespace SourceLink

open System
open System.IO
open System.Collections.Generic
open System.Text
open SourceLink.Extension
open SourceLink.Exception
open SourceLink.File
open SourceLink.PdbModify

type PdbFile(file) =
    
    let fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
    let br = new BinaryReader(fs, Encoding.UTF8, true)
    let bw = new BinaryWriter(fs, Encoding.UTF8, true)

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
        let bytes = Array.create stream.ByteCount 0uy
        let pages = stream.Pages
        if pages.Length <> 0 then
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
            let streams = root.Streams
            for i in 0 .. streamCount - 1 do
                let stream = PdbStream()
                streams.Add stream
                let byteCount = brDirectory.ReadInt32()
                stream.ByteCount <- byteCount
                let pageCount = countPages byteCount
                stream.Pages <- Array.create pageCount 0
            for i in 0 .. streamCount - 1 do
                for j in 0 .. streams.[i].Pages.Length - 1 do
                    let page = brDirectory.ReadInt32()
                    streams.[i].Pages.[j] <- page
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
                for i in 0 .. newPageCount - 1 do
                    let page = (int fs.Position) / pageByteCount
                    pages.Add page
                    fs.WriteBytes zerosPage
        pages

    member x.Dispose() =
        use fs = fs
        use br = br
        use bw = bw
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

    member x.File with get() = file
    member x.ReadPdbStreamBytes pdbStream = readStreamBytes pdbStream
    member x.ReadStreamBytes stream = readStreamBytes root.Streams.[stream]
    member x.Info with get() = info
    member x.HasSrcSrv with get() = info.NameToPdbName.ContainsKey srcsrv
    member x.SrcSrv with get() = info.NameToPdbName.[srcsrv].Stream
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

    member internal x.WriteStream (bytes:byte[]) =
        let n = countPages bytes.Length
        let pages = allocPages n
        for i in 0 .. n - 2 do
            goToPage pages.[i]
            fs.Write(bytes, i * pageByteCount, pageByteCount)
        // write last page
        let last = n-1
        let lastByteCount = 
            let c = bytes.Length % pageByteCount
            if c = 0 then pageByteCount else c
        goToPage pages.[last]
        fs.Write(bytes, last * pageByteCount, lastByteCount)
        let pdbStream = PdbStream()
        pdbStream.ByteCount <- bytes.Length
        pdbStream.Pages <- pages.ToArray()
        pdbStream

    member x.WriteToStream index bytes =
        root.Streams.[index] <- x.WriteStream bytes

    member x.WriteNewStream name bytes =
        let pdbStream = x.WriteStream bytes
        let stream = x.Root.AddStream pdbStream
        let pdbName = x.Info.AddNewName name
        pdbName.Stream <- stream

    member x.WriteRootPage bytes =
        goToPage rootPage
        fs.WriteBytes zerosPage
        goToPage rootPage
        fs.WriteBytes bytes

    member x.WriteHeader (rootByteCount:int) =
        br.Position <- 0x28
        bw.Write ((int fs.Length) / pageByteCount) // pageCount
        bw.Write rootByteCount

    member x.OrphanedPages
        with get() =
            let used = SortedSet<int>()
            let add page = used.Add page |> ignore
            add x.RootPage
            for page in x.RootPdbStream.Pages do
                add page
            for stream in x.Root.Streams do
                for page in stream.Pages do
                    add page
            let orphaned = List<int>()
            // page 0 is header
            for i in 1 .. x.PageCount - 1 do
                if false = used.Contains i then
                    orphaned.Add i
            orphaned.ToArray()

    /// frees pages for info stream, root stream, and orphaned pages
    member x.FreeInfoPages() =
        x.FreePages x.OrphanedPages
        x.FreePages x.RootPdbStream.Pages // free root
        x.FreeStream 1 // free info

    /// writes the info stream, root stream, and header
    member x.Save() =
        let infoBytes = createInfoBytes x.Info
        x.WriteToStream 1 infoBytes
        let rootBytes = createRootBytes x.Root
        let rootPdbStream = x.WriteStream rootBytes
        let rootPageBytes = createRootPageBytes rootPdbStream
        x.WriteRootPage rootPageBytes
        x.WriteHeader rootPdbStream.ByteCount

    member x.WriteSrcSrv bytes =
        if x.HasSrcSrv then
            x.FreeStream x.SrcSrv
            x.WriteToStream x.SrcSrv bytes
        else
            x.WriteNewStream srcsrv bytes

    static member WriteSrcSrvBytesTo bytes toFile =
        use pdb = new PdbFile(toFile)
        pdb.FreeInfoPages()
        pdb.WriteSrcSrv bytes
        pdb.Info.Age <- pdb.Info.Age + 1
        pdb.Save()

    static member WriteSrcSrvFileTo file toFile =
        PdbFile.WriteSrcSrvBytesTo (File.ReadAllBytes file) toFile

    static member ReadSrcSrvBytes file = 
        use pdb = new PdbFile(file)
        if pdb.HasSrcSrv then pdb.ReadStreamBytes pdb.SrcSrv else [||]

    static member ReadSrcSrvLines file =
        use pdb = new PdbFile(file)
        if pdb.HasSrcSrv then pdb.ReadStreamBytes pdb.SrcSrv |> readLines else [||]
         