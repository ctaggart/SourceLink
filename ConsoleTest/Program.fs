open System
//open System.Collections.Generic
open System.IO
open SourceLink
open SourceLink.SymbolStore
open System.Reflection

let pdbSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.pdb"
//let pdbSourceLink = @"C:\Projects\SourceLink1\packages\SourceLink.Fake\tools\SourceLink.pdb"

let printPdbDocuments() =
    use s = File.OpenRead pdbSourceLink

    let pdbSourceLink = Path.GetFullPath pdbSourceLink
    let sc = SymbolCache @"C:\tmp\cache"
    let r = sc.ReadPdb(s, pdbSourceLink)

    for d in r.Documents do
        printfn "\npdb original source file path: %s" d.URL
        printfn "it had an md5 checksum of: %s" (d.GetCheckSum() |> Hex.encode)
        let url = r.GetDownloadUrl d.URL
        printfn "has download url if source indexed: %s" url
//        let downloadedFile = sc.DownloadFile url
//        printfn "downloaded the file to the cache %s" downloadedFile
//        printfn "downloaded file has md5 of: %s" (Crypto.hashMD5 downloadedFile |> Hex.encode)
        ()

let printMethods() =
    let dll = Assembly.LoadFrom @"..\..\..\packages\SourceLink.SymbolStore\lib\net45\SourceLink.SymbolStore.dll"
    for dt in dll.DefinedTypes do
        printfn "\n%s" dt.FullName
        for m in dt.GetMembers() do
            printfn "  %d %s" m.MetadataToken m.Name


// print methods and their files and line numbers
let printMethodsFileLines() =
    let dll = @"..\..\..\packages\SourceLink.SymbolStore\lib\net45\SourceLink.SymbolStore.dll"
    let pdb = Path.ChangeExtension(dll, ".pdb")
    let sc = SymbolCache @"C:\tmp\cache"
    use s = File.OpenRead pdb
    use r = sc.ReadPdb(s, pdb)

    for d in r.Documents do
        for m in d.GetMethods r.ISymUnmanagedReader2 do
            let token = m.Token.GetToken()
            let fn = m.GetFileNameFromOffset 0

            printfn "%d method in %s" token fn
            printfn "  %d sequence points" m.SequencePointCount
//            for p in m.SequencePoints do
//                printfn "%d, %d" p.Line p.Column
        ()
        

//    let a = Assembly.LoadFrom dll
//    for dt in a.DefinedTypes do
//        printfn "\n%s" dt.FullName
//        for m in dt.GetMembers() do
//            printfn "  %d %s" m.MetadataToken m.Name

[<EntryPoint>]
let main argv =
    printPdbDocuments()
//    printMethodsFileLines()
    0
