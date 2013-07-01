
open SourceLink
open SourceLink.Build
open SourceLink.Pdb
open SourceLink.Extension

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

//    printChecksumsStored @"c:\temp\trybuild7" [|"Program.cs"|]
    use file = new PdbFile(@"C:\Projects\pdb\LibGit2Sharp.pdb\01980BA64D5A4977AF82EDC15D5B6DC61\LibGit2Sharp.1.pdb")
    let info = file.Info
    printfn "pdb guid: %s" info.Guid.ToStringN
    for KeyValue(filename, checksum) in info.Filenames do
        printfn "%s %s" checksum filename

    0 // exit code
