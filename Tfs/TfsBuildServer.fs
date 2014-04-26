[<AutoOpen>]
module SourceLink.TfsBuildServer

open System
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow
open Microsoft.TeamFoundation.Build.Common

type IWorkspaceTemplate with
    member x.FirstMapping
        with get() =
            x.Mappings |> Seq.find (fun m -> m.MappingType = WorkspaceMappingType.Map)
    /// returns the Source Control Folder mapped to the Build Agent Folder $(SourceDir)
    member x.SourceDir
        with get() =
            match x.Mappings |> Seq.tryFind (fun m -> m.LocalItem <> null && m.LocalItem.Equals("$(SourceDir)", StringComparison.OrdinalIgnoreCase)) with
            | Some m -> Some m.ServerItem
            | None -> None

type IBuildServer with
    member x.GetBuildControllerUri id = sprintf "vstfs:///Build/Controller/%d" id |> Uri.from
    member x.GetBuildController id = x.GetBuildController(x.GetBuildControllerUri id, false)
    member x.GetBuildDefinitionUri id = sprintf "vstfs:///Build/Definition/%d" id |> Uri.from
    member x.GetBuildDefinition id = x.GetBuildDefinitionUri id |> x.GetBuildDefinition
    member x.GetBuildUri id = sprintf "vstfs:///Build/Build/%d" id |> Uri.from
    member x.GetBuild id = x.GetBuildUri id |> x.GetBuild
    member x.CreateBuildRequest id = x.GetBuildUri id |> x.CreateBuildRequest
    member x.GetBuildDefinitions (projectName:string) = x.QueryBuildDefinitions projectName
    member x.GetProcessTemplate project serverPath = x.QueryProcessTemplates project |> Seq.tryFind (fun pt -> pt.ServerPath.EqualsI serverPath)
            
type IBuildDefinition with
    /// deserializes the xml of parameters
    member x.GetParameters() = TfsProcessParameters(x.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters)
    /// serializes the xml of parameters
    member x.SetParameters (v:TfsProcessParameters) = x.ProcessParameters <- WorkflowHelpers.SerializeProcessParameters v.Dictionary
    static member sortName (a:IBuildDefinition) (b:IBuildDefinition) = String.cmpi a.Name b.Name

type IBuildDetail with
    /// deserializes the xml of parameters
    member x.GetParameters() = TfsProcessParameters(x.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters)
    /// the Git branch and commit, a parsed SourceGetVersion
    member x.SourceVersionGit 
        with get() =
            let _, branch, commit = BuildSourceVersion.TryParseGit x.SourceGetVersion
            branch, commit