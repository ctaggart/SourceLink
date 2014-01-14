[<AutoOpen>]
module Checksums

open System
open System.Collections.Generic
open System.IO
open SourceLink

let printRevision dir =
    use repo = new GitRepo(dir)
    printfn "revision: %s" repo.Revision

let printChecksumsGit dir proj =
    let p = VsProject.Load proj ["Configuration", "Debug"]
    use repo = new GitRepo(dir)
    for file, checksum in repo.Checksums p.ItemsCompile do
        printfn "%s %s" file checksum

let printChecksumsPdb proj =
    let p = VsProject.Load proj ["Configuration", "Debug"]
    use pdb = new PdbFile(p.OutputFilePdb)
    for file, checksum in pdb.Files do
        printfn "%s %s" file (checksum |> Hex.encode)

//printRevision (Path.combine __SOURCE_DIRECTORY__ "..")
//printChecksumsGit (Path.combine __SOURCE_DIRECTORY__ "..") (Path.combine __SOURCE_DIRECTORY__ @"..\Tfs\Tfs.fsproj")
//printChecksumsPdb (Path.combine __SOURCE_DIRECTORY__ @"..\Tfs\Tfs.fsproj")