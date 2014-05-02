namespace SourceLink

open Microsoft.TeamFoundation.Build.Client

type TfsSummarySection(bi:IBuildInformation, priority, key, name) =
    member x.AddMessage format = Printf.ksprintf (fun message -> bi.AddCustomSummaryInformation(message, key, name, priority) |> ignore) format

[<AutoOpen>]
module BuildInformation =
    type IBuildInformation with
        member x.AddSummarySection priority key name = TfsSummarySection(x, priority, key, name)