//#I __SOURCE_DIRECTORY__
//#r "..\..\packages\FAKE.2.4.8.0\Tools\FakeLib.dll"
#load "Assemblies.fsx"

open Fake
open SourceLink

let isTfs = hasBuildParam "tfsBuild"
let getTfsBuild() = new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")

type Microsoft.Build.Evaluation.Project with
    member x.Compiles : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompile
        Excludes = [] }

let verifyChecksums (repo:GitRepo) files =
    let different = repo.VerifyChecksums files
    if different.Length <> 0 then
        let errMsg = sprintf "%d source files do not have matching checksums in the git repository" different.Length
        log errMsg
        for file in different do
            logfn "no checksum match found for %s" file
        failwith errMsg