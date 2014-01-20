#load "Assemblies.fsx"

open System
open Microsoft.TeamFoundation.Build.Client
open Fake
open SourceLink

//// code in progress
//type TfsBuildTraceListener(tb:TfsBuild) =
//    let bi = tb.Build.Information
//    let addActivity displayText start finish =
//        let at = bi.AddActivityTracking(null, null, displayText)
//        at.StartTime <- start
//        at.FinishTime <- finish
//        at.Save()
//    // not showing up
//    let highMessage text = bi.AddBuildMessage("high: "+text, BuildMessageImportance.High, DateTime.UtcNow) |> ignore
//    let normalMessage text = bi.AddBuildMessage("normal: "+text, BuildMessageImportance.Normal, DateTime.UtcNow) |> ignore
//    let lowMessage text = bi.AddBuildMessage("low: "+text, BuildMessageImportance.Low, DateTime.UtcNow) |> ignore
//    let errorMessage text = bi.AddBuildError(text, DateTime.UtcNow) |> ignore
//    let mutable step = null
//    interface ITraceListener with
//        member this.Write msg =
//            match msg with 
//            | StartMessage -> ()
//            | ImportantMessage text -> highMessage text
//            | LogMessage(text,_) -> normalMessage text
//            | TraceMessage(text,_) -> lowMessage text
//            | FinishedMessage -> ()
//            | OpenTag(tag,name) ->
//                step <- bi.AddBuildStep(tag,name)
//                step.StartTime <- DateTime.UtcNow
//            | CloseTag tag ->
//                step.FinishTime <- DateTime.UtcNow
//                step.Save()
//            | ErrorMessage text -> errorMessage text
//            
//if isTfsBuild then
//    listeners.Clear()
//    listeners.Add (new TfsBuildTraceListener(getTfsBuild()))

let isTfsBuild = hasBuildParam "tfsBuild"
let getTfsBuild() =
    if isTfsBuild then new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")
    else Ex.failwithf "isTfsBuild = false"

type Microsoft.Build.Evaluation.Project with
    member x.Compiles : FileIncludes = {
        BaseDirectory = x.DirectoryPath
        Includes = x.ItemsCompile
        Excludes = [] }

let verifyGitChecksums (repo:GitRepo) files =
    let different = repo.VerifyChecksums files
    if different.Length <> 0 then
        let errMsg = sprintf "%d source files do not have matching checksums in the git repository" different.Length
        log errMsg
        for file in different do
            logfn "no checksum match found for %s" file
        failwith errMsg

let verifyPdbChecksums (p:VsProject) files =
    let missing = p.VerifyPdbChecksums files
    if missing.Count > 0 then
        let errMsg = sprintf "cannot find %d source files" missing.Count
        log errMsg
        for file, checksum in missing.KeyValues do
            logfn "cannot find %s with checksum of %s" file checksum
        failwith errMsg