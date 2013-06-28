
open SourceLink
open SourceLink.Build

let printChecksums proj =
//    let sc = SourceCheck()
//    sc.ProjectFile <- proj
//    sc.Execute() |> ignore
    let compiles = Proj.getCompiles proj
    for checksum, file in Git.computeChecksums compiles do
        printfn "%s %s"checksum file

let printRevision dir =
    let r = Git.getRevision dir
    printfn "revision: %s" r

let printChecksumsStored dir files =
    for checksum in Git.getChecksums dir files do
        printfn "%s"checksum

[<EntryPoint>]
let main argv = 

    printChecksumsStored @"c:\temp\trybuild7" [|"Program.cs"|]

    0 // exit code
