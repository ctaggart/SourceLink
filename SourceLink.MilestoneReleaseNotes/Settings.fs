// Settings can be configured using envronment variables or command line arguments
// as described in the ASP.NET Core documentation
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1

// The keys are not case sensitive. example:
// milestonereleasenotes githubtoken=$githubtoken githubowner=ctaggart githubrepository=sourcelink milestone=3.0.0
// You can also use environment variables with a prefix of RELEASENOTES_, such as RELEASENOTES_GITHUBTOKEN

namespace SourceLink.MilestoneReleaseNotes

open System
open Microsoft.Extensions.Configuration

[<CLIMutable>]
type Settings =
    {
        GitHubUrl: string
        GitHubToken: string
        GitHubOwner: string
        GitHubRepository: string
        Milestone: string
    }
with
    static member Default =
        {
            GitHubUrl = "https://api.github.com"
            GitHubToken = String.Empty
            GitHubOwner = String.Empty
            GitHubRepository = String.Empty
            Milestone = String.Empty
        }

    static member Load (args: string[]) =
        let config =
            ConfigurationBuilder()
                .AddEnvironmentVariables("RELEASENOTES_")
                .AddCommandLine(args)
                .Build()
        let settings = Settings.Default
        config.Bind settings
        settings