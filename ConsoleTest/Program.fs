
open System.Collections.Generic
open System.IO
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

let printStreamPages (root:PdbRoot) =
    for i in 0 .. root.Streams.Count - 1 do
        let s = root.Streams.[i]
        printf "%d, %d, " i s.ByteCount
        for p in s.Pages do
            printf "%X " p
        printfn ""

let printOrphanedPages (file:PdbFile) =
    for page in file.OrphanedPages do
        printf "%x " page
    printfn ""

let writeFile file bytes =
    use fs = File.OpenWrite file
    fs.WriteBytes bytes

let printDiffPosition (a:byte[]) (b:byte[]) =
    let n = if a.Length < b.Length then a.Length else b.Length
    for i in 0 .. n - 1 do
        if a.[i] <> b.[i] then
            printfn "%X %X %X" i a.[i] b.[i]

let createCopy file i =
    let ext = Path.GetExtension file
    let copy = Path.ChangeExtension(file, sprintf ".%d%s" i ext)
    if File.Exists copy then File.Delete copy
    File.Copy(file, copy)
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
//    printChecksumsGit @"c:\temp\trybuild7" [|"Program.cs"|]
//    printfn "pdb guid: %s" file.Info.Guid.ToStringN

//    printSrcSrv @"C:\Projects\pdb\Autofac.pdb\Autofac.pdb"
//    printSrcSrv @"C:\Projects\pdb\Autofac.pdb\D77905B67A5046138298AF1CC87D57D51\Autofac.pdb"

//    let file = @"C:\Projects\libgit2sharp\LibGit2Sharp\bin\Release\LibGit2Sharp.pdb"
//    let file2 = createCopy file 2
//    let ss = @"C:\Projects\libgit2sharp\LibGit2Sharp\LibGit2Sharp.pdb.srcsrv.txt"
//    PdbFile.WriteSrcSrvFileTo ss file2

//    printSrcSrv @"C:\Projects\libgit2sharp\LibGit2Sharp\bin\Release\LibGit2Sharp.2.pdb"
    printSrcSrv @"C:\Projects\libgit2sharp\LibGit2Sharp\bin\Release\LibGit2Sharp.pdb"

//    printDia file2

    0 // exit code
