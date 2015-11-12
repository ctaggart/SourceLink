#r "System.Xml"
#r "System.Configuration"

#I __SOURCE_DIRECTORY__
//#r "../packages/FAKE/tools/FakeLib.dll" // in dev
//#r "../packages/LibGit2Sharp/lib/net40/LibGit2Sharp.dll" // in dev
//#r "../SourceLink/bin/Debug/SourceLink.Core.dll" // in dev
//#r "../Git/bin/Debug/SourceLink.Git.dll" // in dev
#r "System.Threading.Tasks.Dataflow.dll"
#r "Microsoft.Build.Framework.dll"
#r "Microsoft.Build.dll"
#r "LibGit2Sharp.dll"
#r "SourceLink.Core.dll"
#r "SourceLink.Git.dll"

open System
open System.IO
open Fake
open SourceLink

let getBuildConfig dir =
    AppConfig.get (Path.combine dir "build.config")

type Microsoft.Build.Evaluation.Project with
    /// all items with a build action of compile
    member x.Compiles : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompilePath
        Excludes = [] }
    /// all items with a build action of compile that are linked
    member x.CompilesLinked : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompileLinkPath
        Excludes = [] }
    /// all items with a build action of compile that are not linked
    member x.CompilesNotLinked : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompilePath
        Excludes = x.ItemsCompileLinkPath }

type GitRepo with
    [<Obsolete "use SourceLink.Index">]
    member x.VerifyChecksums files =
        let different = x.VerifyFiles files
        if different.Length <> 0 then
            let errMsg = sprintf "%d source files do not have matching checksums in the git repository" different.Length
            log errMsg
            for file in different do
                logfn "no checksum match found for %s" file
            failwith errMsg

type Pdbstr with
    static member execWith exe pdb srcsrv =
        let workdir = Path.GetDirectoryName pdb
        // use relative paths if in workdir
        let srcsrv = if workdir.EqualsI <| Path.GetDirectoryName srcsrv then Path.GetFileName srcsrv else srcsrv
        let pdb = if workdir.EqualsI <| Path.GetDirectoryName pdb then Path.GetFileName pdb else pdb
        let args = sprintf "-w -s:srcsrv -i:\"%s\" -p:\"%s\"" srcsrv pdb
        logVerbosefn "%s>\"%s\" %s" workdir exe args
        Shell.Exec(exe, args, workdir) |> ignore
    static member exec pdb srcsrv =
        let exe = Pdbstr.tryFind()
        if exe.IsNone then
            failwith "pdbstr.exe not found, install Debugging Tools for Windows"
        Pdbstr.execWith exe.Value pdb srcsrv

type Microsoft.Build.Evaluation.Project with // VsProj
    [<Obsolete "use SourceLink.Index">]
    member x.VerifyPdbChecksums files =
        let missing = x.VerifyPdbFiles files
        if missing.Count > 0 then
            let errMsg = sprintf "cannot find %d source files" missing.Count
            log errMsg
            for file, checksum in missing.KeyValues do
                logfn "cannot find %s with checksum of %s" file checksum
            failwith errMsg

type SourceLink =
    /// Verifies the checksums for the list of files
    static member Index sourceFiles pdbFile gitRepoPath url =
        logfn "source indexing %s" pdbFile
        let srcsrvFile =
            use pdb = new PdbFile(pdbFile)

            // verify checksums in the pdb 1st
            let pc = pdb.MatchChecksums sourceFiles
            if pc.Unmatched.Count > 0 then
                let errMsg = sprintf "%d source files do not have matching checksums in the pdb file" pc.Unmatched.Count
                log errMsg
                for um in pc.Unmatched do
                    logfn "  pdb %s, file %s %s" um.ChecksumInPdb um.ChecksumOfFile um.File
                failwith errMsg

            // verify checksums in git 2nd
            use repo = new GitRepo(gitRepoPath)
            let gc = repo.MatchChecksums pc.MatchedFiles
            if gc.Unmatched.Count > 0 then
                let errMsg = sprintf "%d source files do not have matching checksums in the Git repository" gc.Unmatched.Count
                log errMsg
                for um in gc.Unmatched do
                    logfn "  git %s, file %s %s" um.ChecksumInGit um.ChecksumOfFile um.File
                failwith errMsg

            let srcsrvFile = pdbFile + ".srcsrv"
            File.WriteAllBytes(srcsrvFile, SrcSrv.create url repo.Commit (repo.Paths pc.MatchedFiles))
            srcsrvFile
        Pdbstr.exec pdbFile srcsrvFile