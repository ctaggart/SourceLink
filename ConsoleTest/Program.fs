module SourceLink.Program

open System
open System.IO
open SourceLink
open SourceLink.SymbolStore
open System.Reflection

//let pdbSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.pdb"
//let dllSourceLink = @"..\..\..\packages\SourceLink.Fake\tools\SourceLink.dll"
let pdbSourceLink = @"..\..\..\packages\Octokit.0.8.0\lib\net45\Octokit.pdb"
let dllSourceLink = @"..\..\..\packages\Octokit.0.8.0\lib\net45\Octokit.dll"
let cachDir = @"..\..\..\packages"

let printMethods() =
    use pdb = File.OpenRead pdbSourceLink
    let symbolCache = SymbolCache cachDir
    let pdbReader = symbolCache.ReadPdb pdb pdbSourceLink
    let dll = Assembly.LoadFrom dllSourceLink
    let md = dll.Modules |> Seq.head
    printfn "module: %s" md.FullyQualifiedName
    
    dll.DefinedTypes
//    |> Seq.filter (fun dt -> dt.FullName = "SourceLink.VsBuild") // F# module
//    |> Seq.filter (fun dt -> dt.FullName = "SourceLink.PdbFile") // class
    |> Seq.iter (fun dt ->
        for mbr in dt.GetMembers() do
            printfn "%s, %s, %d" dt.FullName mbr.Name mbr.MetadataToken
            match pdbReader.GetMethod mbr.MetadataToken with
            | None -> ()
            | Some mth ->
                for sp in mth.SequencePoints do
                    printfn "    %s %d %d" sp.Document.SourceFilePath sp.Line sp.Column
                    let downloadUrl = pdbReader.GetDownloadUrl sp.Document.SourceFilePath |> Option.get
                    printfn "    %s" downloadUrl
                    let browserUrl = sprintf "%s#L%d" (downloadUrl.Replace("https://raw.githubusercontent.com/octokit/octokit.net", "https://github.com/octokit/octokit.net/blob")) sp.Line
                    printfn "    %s" browserUrl 
    )

/// gets all tokens in the pdb file
let printTokens() =
    use pdb = File.OpenRead pdbSourceLink
    let symbolCache = SymbolCache cachDir
    let pdbReader = symbolCache.ReadPdb pdb pdbSourceLink
    let dll = Assembly.LoadFrom dllSourceLink
    let md = dll.Modules |> Seq.head
    printfn "module: %s" md.FullyQualifiedName

    for d in pdbReader.Documents do
        printfn "%s" d.SourceFilePath
        for m in pdbReader.Reader.GetMethodsInDocument d do
            let mth = md.ResolveMember m.Token
            printfn "  %d %s %s" m.Token mth.DeclaringType.FullName mth.Name

//    for m in pdbReader.Reader.Methods do
//        let mth = md.ResolveMember m.Token
//        printfn "%d %s %s" m.Token mth.DeclaringType.FullName mth.Name

[<EntryPoint>]
let main argv =
    printMethods()
    printTokens()
    0
