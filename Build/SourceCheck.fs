namespace SourceLink.Build

open System
open System.IO
open Microsoft.Build.Framework
open LibGit2Sharp
open System.Collections.Generic
open SourceLink

type SourceCheck() =
    inherit Task()

    [<Required>]
    member val ProjectFile = String.Empty with set, get

    [<Required>]
    member val RepoDir = String.Empty with set, get

    member internal x.GetRepoDir() = 
        if Path.IsPathRooted x.RepoDir then
            x.RepoDir.TrimEnd [|'\\'|]
        else
            Path.Combine(Path.GetDirectoryName x.ProjectFile, x.RepoDir).TrimEnd [|'\\'|]

    override x.Execute() =
        let repoDir = x.GetRepoDir()

        x.MessageNormal "project file: %s" x.ProjectFile
        x.MessageNormal "repository: %s" repoDir

        try
            let compiles = Proj.getCompiles x.ProjectFile
            x.MessageHigh "compiles in proj: %d" compiles.Length

            let committedChecksums = Git.getChecksums repoDir compiles
            let different = SortedSet(StringComparer.OrdinalIgnoreCase)
    //        for c in compiles  do
    //            x.MessageHigh "compile file: %s" c
            for checksum, file in Git.computeChecksums compiles do
    //            x.MessageHigh "%s %s"checksum file
                if false = committedChecksums.Contains checksum then
                    different.Add file |> ignore
            if different.Count > 0 then
                x.Error "%d of %d checksums do not match" different.Count compiles.Length
                for file in different do
                    x.Error "checksum does not match for %s" file
            else
                x.MessageHigh "all %d checksums match" compiles.Length
        with
        | :? RepositoryNotFoundException as ex -> x.Error "%s" ex.Message
        | :? SourceLinkException as ex -> x.Error "%s" ex.Message

        not x.HasErrors
    