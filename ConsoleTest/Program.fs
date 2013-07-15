
open System.Collections.Generic
open System.IO
open System.Text
open SourceLink
open SourceLink.Build
open SourceLink.Extension
open SourceLink.PdbModify
open SourceLink.SrcSrv
open Microsoft.Dia
open SourceLink.Dia

let printChecksums proj =
    let compiles = Proj.getCompiles proj (HashSet())
    for checksum, file in Git.computeChecksums compiles do
        printfn "%s %s"checksum file

let printRevision dir =
    let r = Git.getRevision dir
    printfn "revision: %s" r

let printChecksumsGit dir files =
    for checksum in Git.getChecksums dir files do
        printfn "%s"checksum

let printChecksumsPdb (file:PdbFile) =
    let checksums = PdbChecksums(file)
    for KeyValue(filename, checksum) in checksums.FilenameToChecksum do
        printfn "%s %s" checksum filename

let pagesToString (pages:int[]) =
    let sb = StringBuilder()
    sb.Appendf "%d pages: " pages.Length
    for p in pages do 
        sb.Appendf "%X " p
    sb.ToString()

let printStreamPages (file:PdbFile) =
    printfn "root page is %X" file.RootPage
    printfn "root stream, %d bytes, %s" file.RootPdbStream.ByteCount (pagesToString file.RootPdbStream.Pages)
    let root = file.Root
    for i in 0 .. root.Streams.Count - 1 do
        let s = root.Streams.[i]
        printfn "stream %d, %d bytes, %s" i s.ByteCount (pagesToString s.Pages)

let printOrphanedPages (file:PdbFile) =
    for page in file.OrphanedPages do
        printf "%x " page
    printfn ""

let writeFile file bytes =
    use fs = File.OpenWrite file
    fs.WriteBytes bytes

let diffBytes (a:byte[]) (b:byte[]) =
    let n = if a.Length < b.Length then a.Length else b.Length
    for i in 0 .. n - 1 do
        if a.[i] <> b.[i] then
            printfn "%X %X %X" i a.[i] b.[i]

let diffFiles a b =
    diffBytes (File.ReadAllBytes a) (File.ReadAllBytes b)

let copyTo file copy =
    if File.Exists copy then File.Delete copy
    File.Copy(file, copy)

let createCopy file i =
    let ext = Path.GetExtension file
    let copy = Path.ChangeExtension(file, sprintf ".%d%s" i ext)
    copyTo file copy
    copy

let diffStreamPages a b =
    use af = new PdbFile(a)
    use bf = new PdbFile(b)
    let ssa = af.Root.Streams
    let ssb = bf.Root.Streams
    printfn "stream count %d, %d" ssa.Count ssb.Count
    let n = if ssa.Count <= ssb.Count then ssa.Count - 1 else ssb.Count - 1
    for i in 0 .. n do
        let sa = ssa.[i]
        let sb = ssb.[i]
        if sa.ByteCount <> sb.ByteCount then
            printfn "stream %d, different byte count, %X, %X" i sa.ByteCount sb.ByteCount
        if sa.Pages.Length <> sb.Pages.Length then
            printfn "stream %d, different # pages, %d, %d" i sa.Pages.Length sb.Pages.Length
        if false = sa.Pages.CollectionEquals sb.Pages then
            printfn "stream %d, pages not same, %A, %A" i sa.Pages sb.Pages

let diffStreamBytes a b =
    use fa = new PdbFile(a)
    use fb = new PdbFile(b)

    // root stream
    let ra = fa.RootPdbStream
    let rb = fb.RootPdbStream
    let rba = fa.ReadPdbStreamBytes ra
    let rbb = fb.ReadPdbStreamBytes rb
    if false = rba.CollectionEquals rbb then
        printfn "root length, %X <> %X" ra.ByteCount rb.ByteCount
        writeFile (sprintf "%s.root" a) rba
        writeFile (sprintf "%s.root" b) rbb

    // other streams
    let ssa = fa.Root.Streams
    let ssb = fb.Root.Streams
    printfn "stream count %d, %d" ssa.Count ssb.Count
    let n = if ssa.Count <= ssb.Count then ssa.Count - 1 else ssb.Count - 1
    for i in 0 .. n do
        let sa = ssa.[i]
        let sb = ssb.[i]
        let ba = fa.ReadPdbStreamBytes sa
        let bb = fb.ReadPdbStreamBytes sb
        if false = ba.CollectionEquals bb then
            printfn "stream %d length, %X <> %X" i sa.ByteCount sb.ByteCount
            writeFile (sprintf "%s.%d" a i) ba
            writeFile (sprintf "%s.%d" b i) bb

let diffInfoStreams a b =
    use fa = new PdbFile(a)
    use fb = new PdbFile(b)
    let ia = fa.Info
    let ib = fb.Info
    if ia.Version <> ib.Version then
        printfn "Version %d <> %d" ia.Version ib.Version
    if ia.Signature <> ib.Signature then
        printfn "Signature %d <> %d" ia.Signature ib.Signature
    if ia.Guid <> ib.Guid then
        printfn "Signature %A <> %A" ia.Guid ib.Guid
    if ia.Age <> ib.Age then
        printfn "Signature %d <> %d" ia.Age ib.Age
    if ia.NameIndexMax <> ib.NameIndexMax then
        printfn "NameIndexMax %d <> %d" ia.NameIndexMax ib.NameIndexMax
    if false = ia.SrcSrv.CollectionEquals ib.SrcSrv then
        printfn "SrcSrv %A <> %A" ia.SrcSrv ib.SrcSrv
    if false = ia.Tail.CollectionEquals ib.Tail then
        printfn "Tail %A <> %A" ia.Tail ib.Tail

    // compare names
    for n in ia.Names do
        if false = ib.NameToPdbName.ContainsKey n.Name then
            printfn "name only in A %s" n.Name
        else
            let sa = ia.NameToPdbName.[n.Name].Stream
            let sb = ib.NameToPdbName.[n.Name].Stream
            if sa <> sb then
                printfn "diff streams for %s, %d, %d" n.Name sa sb
    for n in ib.Names do
        if false = ia.NameToPdbName.ContainsKey n.Name then
            printfn "name only in B %s" n.Name

    printfn "done compairing info streams"

let printDia file =
    let sn = openPdb file
    let gs = sn.globalScope
    printfn "%A %d" gs.guid gs.age

    let sfs = sn.getTables().SourceFiles
    printfn "# of source files %d" sfs.count
    
    for sf in sfs.toSeq() do
        printfn "%d %s" sf.uniqueId sf.fileName
//        for sym in sf.compilands.toSeq() do
//            printfn "  %s" sym.name
//    
//    for ds in sn.getSeqDebugStreams() do
//        printfn "%A %d" ds.name ds.count

let printSrcSrv file = 
    for line in PdbFile.ReadSrcSrvLines file do
        printfn "%s" line

[<EntryPoint>]
let main argv = 

    let file1 = @"C:\Projects\libgit2sharp\LibGit2Sharp.1.pdb"
    let file2 = @"C:\Projects\libgit2sharp\LibGit2Sharp.2.pdb"
    let file3 = @"C:\Projects\libgit2sharp\LibGit2Sharp.3.pdb"
    let file4 = @"C:\Projects\libgit2sharp\LibGit2Sharp.4.pdb"

    let b1 = @"C:\Projects\SourceLink\SourceLink\bin\Debug\SourceLink.1.pdb" // orig
    let b2 = @"C:\Projects\SourceLink\SourceLink\bin\Debug\SourceLink.2.pdb" // this
    let b3 = @"C:\Projects\SourceLink\SourceLink\bin\Debug\SourceLink.3.pdb" // pdbstr
    let b4 = @"C:\Projects\SourceLink\SourceLink\bin\Debug\SourceLink.4.pdb" // pdbstr with free stream 0

//    let file2 = createCopy file 2
//    let srcsrv = @"C:\Projects\libgit2sharp\LibGit2Sharp.pdb.srcsrv.txt"
    let srcsrv = @"C:\Projects\SourceLink\SourceLink\bin\Debug\srcsrv.txt"

//    copyTo b3 b4
//    PdbFile.WriteSrcSrvFileTo srcsrv b2

//    printSrcSrv file3
//    printDia file2
    
//    do
//        use pdb = new PdbFile(b4)
//        pdb.Defrag()

//    do
//        use pdb = new PdbFile(b4)
//        printStreamPages pdb

//    diffStreamBytes b2 b4
    diffFiles (b2+".1") (b4+".1")
    
    0 // exit code
