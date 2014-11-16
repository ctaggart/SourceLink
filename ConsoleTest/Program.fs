open System
//open System.Collections.Generic
open System.IO
open SourceLink
open SourceLink.SymbolStore
open System.Reflection
open Microsoft.Samples.Debugging.SymbolStore

let pdbSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.pdb"

let printPdbDocuments() =
    use s = File.OpenRead pdbSourceLink

    let symbolCache = SymbolCache @"C:\tmp\cache"
    let pdbReader = symbolCache.ReadPdb(s, pdbSourceLink)

    for d in pdbReader.ISymbolReader.GetDocuments() do
        printfn "\npdb original source file path: %s" d.URL
        printfn "it had an md5 checksum of: %s" (d.GetCheckSum() |> Hex.encode)
        let url = pdbReader.GetDownloadUrl d.URL
        printfn "has download url if source indexed: %s" url
//        let downloadedFile = symbolCache.DownloadFile url
//        printfn "downloaded the file to the cache %s" downloadedFile
//        printfn "downloaded file has md5 of: %s" (Crypto.hashMD5 downloadedFile |> Hex.encode)
        ()

let printMethods() =
    let dll = Assembly.LoadFrom @"..\..\..\packages\SourceLink.SymbolStore\lib\net45\SourceLink.SymbolStore.dll"
    for dt in dll.DefinedTypes do
        printfn "\n%s" dt.FullName
        for m in dt.GetMembers() do
            printfn "  %d %s" m.MetadataToken m.Name

type PdbReader with
    member x.MethodTokens
        with get() =
            seq {
                for d in x.ISymUnmanagedReader.GetDocuments() do
                    for m in x.GetMethodsInDocument d do
                        yield m.GetToken() }
    member x.Methods
        with get() =
            x.MethodTokens
            |> Seq.map (fun t -> SymbolToken t |> x.ISymbolReader.GetMethod)

// print methods and their files and line numbers
let printMethodsFileLines() =
    let dll = @"..\..\..\packages\SourceLink.SymbolStore\lib\net45\SourceLink.SymbolStore.dll"
    let pdb = Path.ChangeExtension(dll, ".pdb")
    let sc = SymbolCache @"C:\tmp\cache"
    use s = File.OpenRead pdb
    use pdbReader = sc.ReadPdb(s, pdb)

//    for doc in pdbReader.ISymUnmanagedReader.GetDocuments() do
////        printfn "%s %s" (doc.GetCheckSum() |> Hex.encode) doc.URL / in managed
//
//        for m in pdbReader.GetMethodsInDocument doc do // unmanaged
//        
//            let token = m.GetToken()
////            let fn = m.GetFileNameFromOffset 0 // in managed MDbg SymMethod
//            printfn "%d method" token
////            printfn "%d method in %s" token fn
////            let pointCount = m.GetSequencePointCount()
////            printfn "  %d sequence points" pointCount
////            for p in m. do
////                printfn "%d, %d" p.Line p.Column

    for m in pdbReader.Methods do
//        printfn "%s" (m.GetNamespace().Name) // NIE
        let count = m.SequencePointCount
        printfn "method %d %d" (m.Token.GetToken()) count
        for s in m.GetSequencePoints() do
            printfn "  point %d, %d" s.Line s.Column
    ()
        

//    let a = Assembly.LoadFrom dll
//    for dt in a.DefinedTypes do
//        printfn "\n%s" dt.FullName
//        for m in dt.GetMembers() do
//            printfn "  %d %s" m.MetadataToken m.Name

[<EntryPoint>]
let main argv =
    printPdbDocuments()
    printMethodsFileLines()
    0
