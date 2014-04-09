open System
//open System.Collections.Generic
open System.IO
//open System.Text
open SourceLink
//open SourceLink.PdbModify
//open SourceLink.SrcSrv

let getNugetExeShas() =
    let f = @"C:\Projects\SourceLink\.nuget\NuGet.exe"
    printfn "calculated: %s" (GitRepo.ComputeChecksum f)
    use r = new GitRepo(@"C:\Projects\SourceLink")
    printfn "in repo: %s" (r.Checksum f)

[<EntryPoint>]
let main argv =
    getNugetExeShas()
//    let mdd = Cor.CorMetaDataDispenser() :> Cor.IMetaDataDispenser
//    printfn "mdd: %A" mdd
    0
