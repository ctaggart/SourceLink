namespace SourceLink

open System
open System.Globalization
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow
open Microsoft.TeamFoundation.Build.Workflow.Services

type TfsBuild(project:TfsProject, agent:IBuildAgent, build:IBuildDetail, activityTracking:Choice<int, IActivityTracking>) =
    
    /// deserializes the xml of parameters, BuildDefinition then Build parameters
    let parameters = lazy (
        let ps = build.BuildDefinition.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters
        build.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters |> ps.AddAll
        TfsProcessParameters(ps) )

    // from FAKE scripts
    new(tfsUri:string, tfsUser:string, tfsAgent:string, tfsBuild:string, nodeId:int) =
        let user =
            if String.IsNullOrEmpty tfsUser then TfsUser()
            else tfsUser |> Hex.decode |> Text.Encoding.UTF8.GetString |> TfsUser.FromSimpleWebToken
        let tfs = new Tfs(Uri tfsUri, user.Credentials)
        let bs = tfs.BuildServer
        let agent = tfsAgent |> Uri |> bs.GetBuildAgent
        let build = tfsBuild |> Uri |> bs.GetBuild
        let tp = new TfsProject(tfs, build.TeamProject)
        new TfsBuild(tp, agent, build, Choice1Of2 nodeId)

    // from TFS Activities
    new(agent:IBuildAgent, build:IBuildDetail, activityTracking:IActivityTracking) =
        let tp = new TfsProject(build.BuildServer.TeamProjectCollection, build.TeamProject)
        new TfsBuild(tp, agent, build, Choice2Of2 activityTracking)

    member x.Project with get() = project
    member x.Agent with get() = agent
    member x.Build with get() = build
    /// parameters from the Build Definition and Build
    member x.Parameters with get() = parameters.Value
    member x.BuildDirectory with get() = agent.GetExpandedBuildDirectory build.BuildDefinition

    member x.InformationNodeId 
        with get() = 
            match activityTracking with 
            | Choice1Of2 id -> id
            | Choice2Of2 tracker -> tracker.Node.Id

    member x.Dispose()= 
        use project = project
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
//    override x.Finalize() = x.Dispose()