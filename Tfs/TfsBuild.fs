namespace SourceLink

open System
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow

type TfsBuild(project:TfsProject, agent:IBuildAgent, build:IBuildDetail) =
    
    // from FAKE scripts
    new(tfsUri:string, tfsUser:string, tfsAgent:string, tfsBuild:string) =
        let user = tfsUser |> Hex.decode |> Text.Encoding.UTF8.GetString |> TfsUser.FromSimpleWebToken
        let tfs = new Tfs(Uri tfsUri, user.Credentials)
        let bs = tfs.BuildServer
        let agent = tfsAgent |> Uri.from |> bs.GetBuildAgent
        let build = tfsBuild |> Uri.from |> bs.GetBuild
        let tp = new TfsProject(tfs, build.TeamProject)
        new TfsBuild(tp, agent, build)

    // from TFS Activities
    new(agent:IBuildAgent, build:IBuildDetail) =
        let tp = new TfsProject(build.BuildServer.TeamProjectCollection, build.TeamProject)
        new TfsBuild(tp, agent, build)

    member x.Project with get() = project
    member x.Agent with get() = agent
    member x.Build with get() = build
    
    /// deserializes the xml of parameters, BuildDefinition then Build parameters
    member x.GetParameters() =
        let ps = x.Build.BuildDefinition.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters
        x.Build.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters |> ps.AddAll
        TfsProcessParameters(ps)

    member x.BuildDirectory with get() = agent.GetExpandedBuildDirectory build.BuildDefinition

    member x.Dispose() =
        use project = project
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
//    override x.Finalize() = x.Dispose()