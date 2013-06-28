namespace SourceLink.Build

open System
open System.IO
open Microsoft.Build.Framework
open LibGit2Sharp
open System.Collections.Generic
open SourceLink

type SourceLink() =
    inherit SourceCheck()

    [<Required>]
    member val TargetPath = String.Empty with set, get

    [<Required>]
    member val RepoUrl = String.Empty with set, get

    override x.Execute() =
        let repoDir = x.GetRepoDir()
        let pdbFile = Path.ChangeExtension(x.TargetPath, ".pdb")

        x.MessageNormal "project file: %s" x.ProjectFile
        x.MessageNormal "repo dir: %s" repoDir
        x.MessageNormal "repo url: %s" x.RepoUrl
        x.MessageNormal "pdb file: %s" pdbFile
        
        if false = File.Exists pdbFile then
            x.Error "can not find pdb: %s" pdbFile
        
        if false = x.HasErrors then

            try
                let revision = Git.getRevision repoDir
                x.MessageHigh "git revision is %s" revision
                
                x.MessageHigh "pdb source linking not done yet" // TODO
                
            with
            | :? RepositoryNotFoundException as ex -> x.Error "%s" ex.Message
            | :? SourceLinkException as ex -> x.Error "%s" ex.Message

        not x.HasErrors
    