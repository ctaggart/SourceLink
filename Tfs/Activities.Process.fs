namespace SourceLink.TfsActivities

open System
open System.Activities
open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.Build.Client

[<BuildActivity(HostEnvironmentOption.Agent)>]
type Process() = 
    inherit CodeActivity()

//    [<RequiredArgument>]
    member val FileName = InArgument<string>() with get, set
    member val Arguments = InArgument<string>() with get, set
    member val WorkingDirectory = InArgument<string>() with get, set

    override x.CacheMetadata(metadata:CodeActivityMetadata) : unit =
        base.CacheMetadata metadata
        metadata.RequireExtension typeof<IBuildDetail>

    member x.FailBuildWithf (context:CodeActivityContext) format =
        Printf.ksprintf (fun message ->
            context.TrackBuildError message
            let build = context.GetExtension<IBuildDetail>()
            build.Status <- BuildStatus.Failed
            build.Save()
        ) format

    override x.Execute(context:CodeActivityContext) : unit =
        let build = context.GetExtension<IBuildDetail>()
        let p = SourceLink.Process()
        p.FileName <- x.FileName.Get context
        let arguments = x.Arguments.Get context
        if String.IsNullOrEmpty arguments = false then 
            p.Arguments <- arguments
        let workdir = x.WorkingDirectory.Get context
        if String.IsNullOrEmpty workdir = false then 
            p.WorkingDirectory <- workdir
        let bm s = context.TrackBuildMessage(s, BuildMessageImportance.High)
        p.Stdout |> Observable.add bm
        p.Stderr |> Observable.add bm
        try
            let exit = p.Run()
            if exit <> 0 then 
                x.FailBuildWithf context "process failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
        with
            | ex -> 
                x.FailBuildWithf context "process failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex
