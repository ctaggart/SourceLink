module SourceLink.Program

open System
open System.IO
open SourceLink
open SourceLink.SymbolStore
open System.Reflection

let pdbSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.pdb"
let dllSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.dll"
let cachDir = @"..\..\..\packages"

let printPdbDocuments() =
    use pdb = File.OpenRead pdbSourceLink
    let symbolCache = SymbolCache cachDir
    let pdbReader = symbolCache.ReadPdb pdb pdbSourceLink
    for d in pdbReader.Documents do
        printfn "\npdb original source file path: %s" d.SourceFilePath
        printfn "it had an md5 checksum of: %s" d.ChecksumHex
        let url = pdbReader.GetDownloadUrl d.SourceFilePath |> Option.get
        printfn "has download url if source indexed: %A" url
        let downloadedFile = symbolCache.DownloadFile url
        printfn "downloaded the file to the cache %s" downloadedFile
        printfn "downloaded file has md5 of: %s" (Crypto.hashMD5 downloadedFile |> Hex.encode)

let printMethods() =
    use pdb = File.OpenRead pdbSourceLink
    let symbolCache = SymbolCache cachDir
    let pdbReader = symbolCache.ReadPdb pdb pdbSourceLink
    let dll = Assembly.LoadFrom dllSourceLink
    dll.DefinedTypes
//    |> Seq.filter (fun dt -> dt.FullName = "SourceLink.VsBuild") // F# module
    |> Seq.filter (fun dt -> dt.FullName = "SourceLink.PdbFile") // class
    |> Seq.iter (fun dt ->
        for mbr in dt.GetMembers() do
            printfn "%s, %s" dt.FullName mbr.Name
            match pdbReader.GetMethod mbr.MetadataToken with
            | None -> ()
            | Some mth ->
                for sp in mth.SequencePoints do
                    printfn "    %s %d %d" sp.Document.SourceFilePath sp.Line sp.Column
                    let downloadUrl = pdbReader.GetDownloadUrl sp.Document.SourceFilePath |> Option.get
                    printfn "    %s" downloadUrl
                    let browserUrl = sprintf "%s#L%d" (downloadUrl.Replace("https://raw.githubusercontent.com/ctaggart/SourceLink", "https://github.com/ctaggart/SourceLink/blob")) sp.Line
                    printfn "    %s" browserUrl 
    )

[<EntryPoint>]
let main argv =
    printPdbDocuments()
    printMethods()
    0
