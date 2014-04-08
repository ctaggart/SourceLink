[<AutoOpen>]
module SourceLink.Activities.TfsActivities

open System.Activities
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.Build.Activities.Extensions

type CodeActivityMetadata with
    member x.RequireBuildDetail() = x.RequireExtension typeof<IBuildDetail>
    member x.RequireBuildAgent() = x.RequireExtension typeof<IBuildAgent>

type CodeActivityContext with
    member x.BuildDetail = x.GetExtension<IBuildDetail>()
    member x.BuildAgent = x.GetExtension<IBuildAgent>()
    member x.EnvironmentVariable = x.GetExtension<IEnvironmentVariableExtension>()

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

    member x.BuildDirectory with get() = x.EnvironmentVariable.GetEnvironmentVariable(x, WellKnownEnvironmentVariables.BuildDirectory) :?> string
    member x.SourcesDirectory with get() = x.EnvironmentVariable.GetEnvironmentVariable(x, WellKnownEnvironmentVariables.SourcesDirectory) :?> string
    member x.BinariesDirectory with get() = x.EnvironmentVariable.GetEnvironmentVariable(x, WellKnownEnvironmentVariables.BinariesDirectory) :?> string
    member x.DropLocation with get() = x.EnvironmentVariable.GetEnvironmentVariable(x, WellKnownEnvironmentVariables.DropLocation) :?> string
