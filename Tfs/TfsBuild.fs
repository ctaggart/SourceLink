[<AutoOpen>]
module SourceLink.Tfs.TfsBuild

open System
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow
open SourceLink

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
    member x.GetBuildControllerUri id = Uri(sprintf "vstfs:///Build/Controller/%d" id)
    member x.GetBuildController id = x.GetBuildController(x.GetBuildControllerUri id, false)
    member x.GetBuildDefinitionUri id = Uri(sprintf "vstfs:///Build/Definition/%d" id)
    member x.GetBuildDefinition id = x.GetBuildDefinitionUri id |> x.GetBuildDefinition
    member x.GetBuildUri id = Uri(sprintf "vstfs:///Build/Build/%d" id)
    member x.GetBuild id = x.GetBuildUri id |> x.GetBuild
    member x.CreateBuildRequest id = x.GetBuildUri id |> x.CreateBuildRequest
    member x.GetBuildDefinitions (projectName:string) = x.QueryBuildDefinitions projectName
    member x.GetProcessTemplate project serverPath = x.QueryProcessTemplates project |> Seq.tryFind (fun pt -> pt.ServerPath.EqualsI serverPath)
            
type IBuildDefinition with
    member x.Parameters
        with get() = TfsProcessParameters(x.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters)
        and set (v:TfsProcessParameters) = x.ProcessParameters <- WorkflowHelpers.SerializeProcessParameters v.Dictionary
