
open System.Collections.Generic
open System.IO
open SourceLink
open SourceLink.Build
open SourceLink.Extension
open SourceLink.PdbModify
open SourceLink.SrcSrv

let printChecksums proj =
    let compiles = Proj.getCompiles proj
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

let printStreamPages (root:PdbRoot) =
    for i in 0 .. root.Streams.Count - 1 do
        let s = root.Streams.[i]
        printf "%d, %d, " i s.ByteCount
        for p in s.Pages do
            printf "%X " p
        printfn ""

let printOrphanedPages (file:PdbFile) =
    let used = SortedSet<int>()
    let add page = used.Add page |> ignore
    add file.RootPage
    for page in file.RootPdbStream.Pages do
        add page
    for stream in file.Root.Streams do
        for page in stream.Pages do
            add page
    let pagesFree = List<int>()
    // page 0 is header
    // page 1 is COFFFFFF... // does it need to be?
    for i in 2 .. file.PageCount - 1 do
        if false = used.Contains i then
            pagesFree.Add i
            printf "%x " i
    printfn ""

let writeFile file bytes =
    use fs = File.OpenWrite file
    fs.WriteBytes bytes

let printDiffPosition (a:byte[]) (b:byte[]) =
    let n = if a.Length < b.Length then a.Length else b.Length
    for i in 0 .. n - 1 do
        if a.[i] <> b.[i] then
            printfn "%X %X %X" i a.[i] b.[i]

let createCopy fn =
    let fn0 = Path.ChangeExtension(fn,"0.pdb")
    if File.Exists fn0 then File.Delete fn0
    File.Copy(fn, fn0)
    fn0

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
//        else
//            printfn "stream %d, same byte count, %X" i sa.ByteCount
        if sa.Pages.Length <> sb.Pages.Length then
            printfn "  different # pages, %d, %d" sa.Pages.Length sb.Pages.Length
//        else
//            printfn "  same # pages, %d" sa.Pages.Length
        if false = sa.Pages.CollectionEquals sb.Pages then
            printfn "  pages not same, %A, %A" sa.Pages sb.Pages

let diffStreamBytes a b =
    use fa = new PdbFile(a)
    use fb = new PdbFile(b)

    // root stream
    let ra = fa.RootPdbStream
    let rb = fb.RootPdbStream
    let rba = fa.ReadPdbStreamBytes ra
    let rbb = fb.ReadPdbStreamBytes rb
    if false = rba.CollectionEquals rbb then
        printfn "root length, a %d, b %d" ra.ByteCount rb.ByteCount
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
            printfn "stream %d length, a %d, b %d" i sa.ByteCount sb.ByteCount
            writeFile (sprintf "%s.%d" a i) ba
            writeFile (sprintf "%s.%d" b i) bb

[<EntryPoint>]
let main argv = 
//    printChecksumsGit @"c:\temp\trybuild7" [|"Program.cs"|]

//    use file = new PdbFile(@"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.1.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.2.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\Autofac.pdb\D77905B67A5046138298AF1CC87D57D51\Autofac.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\Autofac.pdb\D864089AF4054AC38A575550C13670FC1\Autofac.pdb")
//    use file = new PdbFile(@"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb")

//    printfn "pdb guid: %s" file.Info.Guid.ToStringN
    
//    printStreamPages file.RootStream
//    printStreamPages file.Stream0
//    printOrphanedPages file

    let fn = @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb"
//    let fn = @"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.1.pdb"
//    let fn0 = @"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.2.pdb"

    do 
        let fn0 = createCopy fn
        use pdb = new PdbFile(fn0)

        writeSrcSrvBytes pdb (File.ReadAllBytes @"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.pdb.srcsrv.txt")
        pdb.Info.Age <- pdb.Info.Age + 1
        pdb.Save()

//    use pdb = new PdbFile(@"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.0.pdb")
    diffStreamPages @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb" @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.0.pdb"
//    diffStreamBytes @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb" @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.0.pdb"

    
    0 // exit code
