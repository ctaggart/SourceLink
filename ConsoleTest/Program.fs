open System
//open System.Collections.Generic
open System.IO
//open System.Text
open SourceLink
//open SourceLink.PdbModify
//open SourceLink.SrcSrv
open SourceLink.SymbolStore

let getNugetExeShas() =
    let f = @"C:\Projects\SourceLink\.nuget\NuGet.exe"
    printfn "calculated: %s" (GitRepo.ComputeChecksum f)
    use r = new GitRepo(@"C:\Projects\SourceLink")
    printfn "in repo: %s" (r.Checksum f)

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
            for p in m.SequencePoints do
                printfn "%d, %d" p.Line p.Column
        ()
        

//    let a = Assembly.LoadFrom dll
//    for dt in a.DefinedTypes do
//        printfn "\n%s" dt.FullName
//        for m in dt.GetMembers() do
//            printfn "  %d %s" m.MetadataToken m.Name

[<EntryPoint>]
let main argv =
//    getNugetExeShas()
//    let mdd = Cor.CorMetaDataDispenser() :> Cor.IMetaDataDispenser
//    printfn "mdd: %A" mdd
    printMethodsFileLines()
    0
