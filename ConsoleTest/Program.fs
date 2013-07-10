
open System.Collections.Generic
open System.IO
open SourceLink
open SourceLink.Build
open SourceLink.Extension
open SourceLink.PdbModify

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
    for i in 0 .. root.Streams.Length - 1 do
        let s = root.Streams.[i]
        printf "%d, %d, " i s.ByteCount
        for p in s.Pages do
            printf "%X " p
        printfn ""
    ()

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

let writeFile file (bytes:byte[]) =
    use fs = File.OpenWrite file
    fs.Write(bytes, 0, bytes.Length)

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

let readAndWriteRoot fn =
    let fn0 = createCopy fn
    use pdb = new PdbFile(fn0)
    pdb.FreePages pdb.RootPdbStream.Pages
    let rootPdbStream = pdb.WriteStream (createRootBytes pdb.Root)
    pdb.WriteRootPage (createRootPageBytes rootPdbStream)
    fn0

let readAndWriteInfo fn =
    let fn0 = createCopy fn
    use pdb = new PdbFile(fn0)
    pdb.FreeStream 1
    pdb.WriteStream (createInfoBytes pdb.Info) |> ignore
    fn0

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
    
//    let bytesOrig = file.ReadStreamBytes 1
//    let bytesMod = createInfoBytes file.Info
//    let bytesOrig = file.ReadPdbStreamBytes file.RootPdbStream
//    let bytesMod = createRootBytes file.Root
//
//    writeFile "root.orig.1" bytesOrig
//    writeFile "root.mod.1" bytesMod


    let fn = @"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb"
//    let fn0 = readAndWriteRoot fn
    let fn0 = readAndWriteInfo fn
    let bytesOrig = File.ReadAllBytes fn
    let bytesMod = File.ReadAllBytes fn0

    let same = bytesOrig.CollectionEquals bytesMod
    if not same then
        printDiffPosition bytesOrig bytesMod


    0 // exit code
