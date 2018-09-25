module SourceLink.MilestoneReleaseNotes.Program

open System
open Octokit
open FSharp.Control.Tasks.V2

let appName = "SourceLink"

type String with
    static member equalsi a b = StringComparer.OrdinalIgnoreCase.Equals(a, b)

[<EntryPoint>]
let main argv =
    let t =
        task {
            try
                let settings = Settings.Load argv
    
                // Oktokit is used to find the milestone
                // https://github.com/octokit/octokit.net/blob/master/docs/getting-started.md

                let client = GitHubClient(ProductHeaderValue appName, Uri settings.GitHubUrl)
                let tokenAuth = Credentials settings.GitHubToken

                client.Credentials <- tokenAuth
                let milestoneRequest = MilestoneRequest()
                milestoneRequest.State <- ItemStateFilter.All
                let! milestones = client.Issue.Milestone.GetAllForRepository(settings.GitHubOwner, settings.GitHubRepository, milestoneRequest)
                let milestone = milestones |> Seq.find (fun milestone -> String.equalsi settings.Milestone milestone.Title)

                let issueRequest = RepositoryIssueRequest ()
                issueRequest.State <- ItemStateFilter.All
                issueRequest.Milestone <- sprintf "%d" milestone.Number
                let! issues = client.Issue.GetAllForRepository(settings.GitHubOwner, settings.GitHubRepository, issueRequest)

                printfn "## Issues"
                issues
                |> Seq.filter (fun issue -> isNull issue.PullRequest)
                |> Seq.iter (fun issue -> printfn "- [#%d](%s) %s" issue.Number issue.HtmlUrl issue.Title)

                printfn "\n## Pull Requests"
                issues
                |> Seq.filter (fun issue -> not <| isNull issue.PullRequest)
                |> Seq.iter (fun issue -> printfn "- [#%d](%s) %s" issue.Number issue.HtmlUrl issue.Title)

                return 0
            with e ->
                eprintfn "failure %A" e
                return 1
        }
    match t.Wait (TimeSpan.FromSeconds 60.) with
    | false ->
        eprintfn "timed out"
        2
    | true ->
        t.Result