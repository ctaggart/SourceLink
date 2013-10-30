[<AutoOpen>]
module SourceLink.TfsActivities.Extensions

open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.Build.Client

type System.Activities.CodeActivityContext with
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
