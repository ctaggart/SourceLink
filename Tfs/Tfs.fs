module SourceLink.Tfs

open System
open System.Collections.Generic
// not sure why opening this requires a reference to assembly 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//open Microsoft.TeamFoundation.Client
open Microsoft.TeamFoundation
open Microsoft.TeamFoundation.Framework.Client
open Microsoft.TeamFoundation.Framework.Common
open Microsoft.TeamFoundation.Build.Client

// abbreviations
//type TfsServer = Client.TfsConfigurationServer 
//type TfsProjectCollection = Client.TfsTeamProjectCollection

type Client.TfsConfigurationServer with
    // available services http://msdn.microsoft.com/en-us/library/bb286958.aspx
    member x.FoundationRegistry with get() = x.GetService<ITeamFoundationRegistry>()
    member x.IdentityManagementService with get() = x.GetService<IIdentityManagementService>()
    member x.JobService with get() = x.GetService<ITeamFoundationJobService>()
    member x.PropertyService with get() = x.GetService<IPropertyService>()
    member x.EventService with get() = x.GetService<IEventService>()
    member x.SecurityService with get() = x.GetService<ISecurityService>()
    member x.LocationService with get() = x.GetService<ILocationService>()
    member x.HyperlinkService with get() = x.GetService<Client.TswaClientHyperlinkService>()
    member x.ProjectCollectionService with get() = x.GetService<ITeamProjectCollectionService>() // TfsConfigurationServer only
    member x.AdministrationService with get() = x.GetService<IAdministrationService>()
    member x.CatalogService with get() = x.GetService<ICatalogService>() // TfsConfigurationServer only

    // requires permission: edit instance-level information
    member x.ProjectCollections with get() = x.ProjectCollectionService.GetCollections()
        
    member x.ProjectCollectionNames
        with get() = 
            x.CatalogNode.QueryChildren([|CatalogResourceTypes.ProjectCollection|], false, CatalogQueryOptions.None)
            |> Array.ofSeq |> Array.map (fun node -> node.Resource.DisplayName)

type Client.TfsTeamProjectCollection with
    // available services http://msdn.microsoft.com/en-us/library/bb286958.aspx
    member x.FoundationRegistry with get() = x.GetService<ITeamFoundationRegistry>()
    member x.IdentityManagementService with get() = x.GetService<IIdentityManagementService>()
    member x.JobService with get() = x.GetService<ITeamFoundationJobService>()
    member x.PropertyService with get() = x.GetService<IPropertyService>()
    member x.EventService with get() = x.GetService<IEventService>()
    member x.SecurityService with get() = x.GetService<ISecurityService>()
    member x.LocationService with get() = x.GetService<ILocationService>()
    member x.HyperlinkService with get() = x.GetService<Client.TswaClientHyperlinkService>()
    member x.AdministrationService with get() = x.GetService<IAdministrationService>()
    member x.VersionControlServer with get() = x.GetService<VersionControl.Client.VersionControlServer>() // TfsTeamProjectCollection only
    member x.WorkItemStore with get() = x.GetService<WorkItemTracking.Client.WorkItemStore>() // TfsTeamProjectCollection only
    member x.BuildServer with get() = x.GetService<Build.Client.IBuildServer>() // TfsTeamProjectCollection only
    member x.TestManagementService with get() = x.GetService<TestManagement.Client.ITestManagementService>() // TfsTeamProjectCollection only
    member x.Linking with get() = x.GetService<ILinking>() // TfsTeamProjectCollection only
    member x.CommonStructureService3 with get() = x.GetService<Server.ICommonStructureService3>() // TfsTeamProjectCollection only
    member x.ServerStatusService with get() = x.GetService<Server.IServerStatusService>() // TfsTeamProjectCollection only
    member x.ProcessTemplates with get() = x.GetService<Server.IProcessTemplates>() // TfsTeamProjectCollection only

    // not working with TF Service
    member x.ProjectNames
        with get() =
            x.VersionControlServer.GetAllTeamProjects false
            |> Array.map (fun p -> p.Name)

type IWorkspaceTemplate with
    member x.FirstMapping
        with get() =
            x.Mappings |> Seq.find (fun m -> m.MappingType = WorkspaceMappingType.Map)
    member x.SourceDirMapping
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
            
type TfsProcessParameters(prms:(IDictionary<string,obj>)) =
    new() = TfsProcessParameters(Dictionary<string,obj>())
    member x.Dictionary with get() = prms
    member x.Add k v = prms.Add(k,v)
    member x.AddString k (v:string) = x.Add k (box v)
    member x.AddInt k (v:int) = x.Add k (box v)
    member x.AddBool k (v:bool) = x.Add k (box v)
//        member x.Get k = prms.

type IBuildDefinition with
    member x.GetParameters() = TfsProcessParameters(x.ProcessParameters |> Build.Workflow.WorkflowHelpers.DeserializeProcessParameters)
    member x.SetParameters (v:TfsProcessParameters) = x.ProcessParameters <- Build.Workflow.WorkflowHelpers.SerializeProcessParameters v.Dictionary

module Tfs =
//    let connectTfsServer (uri:string) = new Client.TfsConfigurationServer(Uri uri)
    let connectTfs (uri:string) = new Client.TfsTeamProjectCollection(Uri uri)



