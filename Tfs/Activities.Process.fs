namespace SourceLink.Tfs.Activities

open System
open System.Activities
open Microsoft.TeamFoundation.Build.Client
open SourceLink
open SourceLink.Tfs

[<BuildActivity(HostEnvironmentOption.Agent)>]
type Process() = 
    inherit CodeActivity()

    member val WorkingDirectory = InArgument<string>() with get, set
    [<RequiredArgument>]
    member val FileName = InArgument<string>() with get, set
    member val Arguments = InArgument<string>() with get, set

    override x.CacheMetadata(metadata:CodeActivityMetadata) : unit =
        base.CacheMetadata metadata
        metadata.RequireBuildDetail()

    override x.Execute(context:CodeActivityContext) : unit =
        let build = context.BuildDetail

        let workdir =
            let wd = x.WorkingDirectory.Get context
            if String.IsNullOrEmpty wd then "" else wd

        let filename = x.FileName.Get context

        let arguments = 
            let args = x.Arguments.Get context
            if String.IsNullOrEmpty args then "" else sprintf " %s" args

        context.MessageHigh "%s>%s%s" workdir filename arguments

        let p = SourceLink.Process()
        p.FileName <- filename
        if String.IsNullOrEmpty arguments = false then 
            p.Arguments <- arguments
        if String.IsNullOrEmpty workdir = false then 
            p.WorkingDirectory <- workdir
        let bm s = context.MessageHigh "%s" s
        p.Stdout |> Observable.add bm
        p.Stderr |> Observable.add bm
        try
            let exit = p.Run()
            if exit <> 0 then 
                context.FailBuildWith "process failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
        with
            | ex -> 
                context.FailBuildWith "process failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex
