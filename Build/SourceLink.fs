namespace SourceLink.Build

open System
open System.IO
open Microsoft.Build.Framework
open LibGit2Sharp
open System.Collections.Generic
open SourceLink
open SourceLink.SrcSrv

type SourceLink() =
    inherit SourceCheck()

    [<Required>]
    member val TargetPath = String.Empty with set, get

    [<Required>]
    member val RepoUrl = String.Empty with set, get

    member val WriteSrcSrvTxt = false with set, get

    override x.Execute() =
        let repoDir = x.GetRepoDir()
        let pdbFile = Path.ChangeExtension(x.TargetPath, ".pdb")
        
        if false = File.Exists pdbFile then
            x.Error "can not find pdb: %s" pdbFile
        
        if false = x.HasErrors then
            try
                use repo = new GitRepo(repoDir)
                let revision = repo.Revision
                x.MessageHigh "source linking %s to %s" pdbFile (createSrcSrvTrg x.RepoUrl revision)
                let files = x.GetSourceFiles()
                use pdb = new PdbFile(pdbFile)
                let missing = pdb.VerifyChecksums files
                if missing.Count > 0 then
                    x.Error "cannot find %d source files" missing.Count
                    for file, checksum in missing.KeyValues do
                        x.Error "cannot find %s with checksum of %s" file checksum

                  

//                if false = x.HasErrors then

//                    let path = file.Substring(repoDir.Length+1).Replace('\\','/')
//                    let srcFiles = sourceFiles |> Seq.map (fun (KeyValue(file,path)) -> file, path) |> Seq.toArray

                      // create SrcSrv
//                    let srcsrv = SrcSrv.createSrcSrv x.RepoUrl revision srcFiles
//                    if x.WriteSrcSrvTxt then File.WriteAllBytes(pdbFile + ".srcsrv.txt", srcsrv)

                      // modify pdb file, use pdbstr if found
//                    pdb.FreeInfo()
//                    pdb.WriteSrcSrv srcsrv
//                    pdb.Info.Age <- pdb.Info.Age + 1
//                    pdb.SaveInfo()
                
            with
            | :? RepositoryNotFoundException as ex -> x.Error "%s" ex.Message
            | :? SourceLinkException as ex -> x.Error "%s" ex.Message
        not x.HasErrors
    