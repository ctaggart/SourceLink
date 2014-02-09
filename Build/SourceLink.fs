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
        
        if File.Exists pdbFile = false then
            x.Error "can not find pdb: %s" pdbFile
        
        if x.HasErrors = false then
            try
                use repo = new GitRepo(repoDir)
                let revision = repo.Revision
                x.MessageHigh "source linking %s to %s" pdbFile (SrcSrv.createTrg x.RepoUrl revision)
                let files = x.GetSourceFiles()
                do
                    use pdb = new PdbFile(pdbFile)
                    let missing = pdb.VerifyChecksums files
                    if missing.Count > 0 then
                        x.Error "cannot find %d source files" missing.Count
                        for file, checksum in missing.KeyValues do
                            x.Error "cannot find %s with checksum of %s" file checksum
                    if x.HasErrors = false then
                        pdb.CreateSrcSrv x.RepoUrl revision (repo.Paths files)
                SrcSrv.write pdbFile (pdbFile + ".srcsrv")
            with
            | :? RepositoryNotFoundException as ex -> x.Error "%s" ex.Message
            | :? SourceLinkException as ex -> x.Error "%s" ex.Message
        not x.HasErrors