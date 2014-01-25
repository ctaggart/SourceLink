#load "Assemblies.fsx"

open System
open System.IO
open Fake
open SourceLink

let isTfsBuild = hasBuildParam "tfsBuild"
#if MONO
#else
let tfsBuild =
    if isTfsBuild then new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")
    else Ex.failwithf "isTfsBuild = false"
#endif

let getBuildConfig dir =
    AppConfig.Get (Path.combine dir "build.config")

type Microsoft.Build.Evaluation.Project with
    member x.Compiles : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompile
        Excludes = [] }

type GitRepo with
    member x.VerifyChecksums files =
        let different = x.VerifyFiles files
        if different.Length <> 0 then
            let errMsg = sprintf "%d source files do not have matching checksums in the git repository" different.Length
            log errMsg
            for file in different do
                logfn "no checksum match found for %s" file
            failwith errMsg

type Microsoft.Build.Evaluation.Project with // VsProj
    member x.VerifyPdbChecksums files =
        let missing = x.VerifyPdbFiles files
        if missing.Count > 0 then
            let errMsg = sprintf "cannot find %d source files" missing.Count
            log errMsg
            for file, checksum in missing.KeyValues do
                logfn "cannot find %s with checksum of %s" file checksum
            failwith errMsg

type Pdbstr with
    static member exec pdb srcsrv =
        let cmd = Pdbstr.tryFind()
        if cmd.IsNone then
            Ex.failwithf "unable to find pdbstr.exe"
        let cmd = cmd.Value
        let args = sprintf "-w -s:srcsrv -i:%s -p:%s" (Path.GetFileName srcsrv) (Path.GetFileName pdb)
        let workdir = Path.GetDirectoryName pdb
//        logfn "%s>%s %s" workdir cmd args
        Shell.Exec(cmd, args, workdir) |> ignore