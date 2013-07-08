
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

let printStreamPages (rootStream:RootStream) =
    for i in 0 .. rootStream.Streams.Length - 1 do
        let s = rootStream.Streams.[i]
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
    for stream in file.RootStream.Streams do
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

[<EntryPoint>]
let main argv = 

//    printChecksumsGit @"c:\temp\trybuild7" [|"Program.cs"|]

//    use file = new PdbFile(@"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.1.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.2.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\Autofac.pdb\D77905B67A5046138298AF1CC87D57D51\Autofac.pdb")
//    use file = new PdbFile(@"C:\Projects\pdb\Autofac.pdb\D864089AF4054AC38A575550C13670FC1\Autofac.pdb")
    use file = new PdbFile(@"C:\Projects\SourceLink\ConsoleTest\bin\Debug\SourceLink - Copy.pdb")

//    printfn "pdb guid: %s" file.Info.Guid.ToStringN
    
//    printStreamPages file.RootStream
//    printStreamPages file.Stream0
//    printOrphanedPages file
    
    let bytesOrig = file.ReadStreamBytes 1
    let bytesMod = createInfoBytes file.Info
//    writeFile "bytesOrig.3" bytesOrig
//    writeFile "bytesMod.4" bytesMod
    let same = bytesOrig.CollectionEquals bytesMod
    if not same then
        printDiffPosition bytesOrig bytesMod

    0 // exit code
