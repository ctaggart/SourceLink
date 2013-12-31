namespace SourceLink

open System
open Microsoft.TeamFoundation.Build.Client

type TfsBuild(project:TfsProject, agent:IBuildAgent, build:IBuildDetail) =

    new(tfsUri:string, tfsUser:string, tfsAgent:string, tfsBuild:string) =
        let user = tfsUser |> Hex.decode |> Text.Encoding.UTF8.GetString |> TfsUser.FromSimpleWebToken
        let tp = new TfsProject(Uri tfsUri, user)
        let bs = tp.Tfs.BuildServer
        let agent = tfsAgent |> Uri.from |> bs.GetBuildAgent
        let build = tfsBuild |> Uri.from |> bs.GetBuild
        new TfsBuild(tp, agent, build)

    member x.Project with get() = project
    member x.Agent with get() = agent
    member x.Build with get() = build

    member x.Dispose() =
        use project = project
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

