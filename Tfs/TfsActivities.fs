[<AutoOpen>]
module SourceLink.Activities.TfsActivities

open System.Activities
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow.Activities

type CodeActivityMetadata with
    member x.RequireBuildDetail() = x.RequireExtension typeof<IBuildDetail>
    member x.RequireBuildAgent() = x.RequireExtension typeof<IBuildAgent>

type CodeActivityContext with
    member x.BuildDetail = x.GetExtension<IBuildDetail>()
    member x.BuildAgent = x.GetExtension<IBuildAgent>()

    member x.FailBuildWith format =
        Printf.ksprintf (fun message ->
            x.TrackBuildError message
            let build = x.GetExtension<IBuildDetail>()
            build.Status <- BuildStatus.Failed
            build.Save()
        ) format

    member x.MessageHigh format = Printf.ksprintf (fun message -> x.TrackBuildMessage(message, BuildMessageImportance.High)) format
    member x.MessageNormal format = Printf.ksprintf (fun message -> x.TrackBuildMessage(message, BuildMessageImportance.Normal)) format
    member x.MessageLow format = Printf.ksprintf (fun message -> x.TrackBuildMessage(message, BuildMessageImportance.Low)) format