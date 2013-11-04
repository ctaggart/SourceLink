module SourceLink.Tfs

open System
open System.Collections.Generic
// not sure why opening this requires a reference to assembly 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
//open Microsoft.TeamFoundation.Client
open Microsoft.TeamFoundation
open Microsoft.TeamFoundation.Framework.Client
open Microsoft.TeamFoundation.Framework.Common
open Microsoft.TeamFoundation.Build.Client
open Microsoft.TeamFoundation.Build.Workflow
open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.VersionControl.Client
open Microsoft.TeamFoundation.VersionControl.Common

// abbreviations
type Tfs = Client.TfsTeamProjectCollection
type TfsConfig = Client.TfsConfigurationServer

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
            
type TfsProcessParameters(prms:(IDictionary<string,obj>)) =
    new() = TfsProcessParameters(Dictionary<string,obj>())
    member x.Dictionary with get() = prms
    member x.Add k v = prms.Add(k,v)
    member x.AddString k (v:string) = x.Add k (box v)
    member x.AddInt k (v:int) = x.Add k (box v)
    member x.AddBool k (v:bool) = x.Add k (box v)
    member x.Get k = prms.Get k
    member x.Set k v = prms.Set k v
    member x.Remove (k:string) = prms.Remove k |> ignore
    member x.Item
        with get k = prms.[k]
        and set k (v:obj) = prms.[k] <- v

    // TFVC and Git
    member x.BuildNumberFormat 
        with get() = match x.Get "BuildNumberFormat" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["BuildNumberFormat"] <- v | None -> x.Remove "BuildNumberFormat"
    member x.AgentSettings 
        with get() = match x.Get "AgentSettings" with | Some o -> o :?> AgentSettings |> Some | None -> None
        and set (value:option<AgentSettings>) = match value with | Some v -> x.["AgentSettings"] <- v | None -> x.Remove "AgentSettings"
    member x.MSBuildArguments 
        with get() = match x.Get "MSBuildArguments" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["MSBuildArguments"] <- v | None -> x.Remove "MSBuildArguments"
    member x.MSBuildPlatform 
        with get() = match x.Get "MSBuildPlatform" with | Some o -> o :?> ToolPlatform |> Some | None -> None
        and set (value:option<ToolPlatform>) = match value with | Some v -> x.["MSBuildPlatform"] <- v | None -> x.Remove "MSBuildPlatform"
    member x.MSBuildMultiProc 
        with get() = match x.Get "MSBuildMultiProc" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["MSBuildMultiProc"] <- v | None -> x.Remove "MSBuildMultiProc"
    member x.Verbosity 
        with get() = match x.Get "Verbosity" with | Some o -> o :?> BuildVerbosity |> Some | None -> None
        and set (value:option<BuildVerbosity>) = match value with | Some v -> x.["Verbosity"] <- v | None -> x.Remove "Verbosity"
    member x.Metadata 
        with get() = match x.Get "Metadata" with | Some o -> o :?> ProcessParameterMetadataCollection |> Some | None -> None
        and set (value:option<ProcessParameterMetadataCollection>) = match value with | Some v -> x.["Metadata"] <- v | None -> x.Remove "Metadata"
    member x.SupportedReasons 
        with get() = match x.Get "SupportedReasons" with | Some o -> o :?> BuildReason |> Some | None -> None
        and set (value:option<BuildReason>) = match value with | Some v -> x.["SupportedReasons"] <- v | None -> x.Remove "SupportedReasons"
    member x.BuildProcessVersion 
        with get() = match x.Get "BuildProcessVersion" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["BuildProcessVersion"] <- v | None -> x.Remove "BuildProcessVersion"

    // TFVC
    member x.BuildSettings 
        with get() = match x.Get "BuildSettings" with | Some o -> o :?> BuildSettings |> Some | None -> None
        and set (value:option<BuildSettings>) = match value with | Some v -> x.["BuildSettings"] <- v | None -> x.Remove "BuildSettings"
    member x.ProjectsToBuild
        with get() =
            if x.BuildSettings.IsNone then None 
            else if x.BuildSettings.Value.HasProjectsToBuild = false then None
            else
                let projects = x.BuildSettings.Value.ProjectsToBuild.ToArray()
                if projects.Length = 0 then None else Some projects
    member x.PlatformConfigurations
        with get() =
            if x.BuildSettings.IsNone then None 
            else if x.BuildSettings.Value.HasPlatformConfigurations = false then None
            else
                let configurations = x.BuildSettings.Value.PlatformConfigurations.ToArray()
                if configurations.Length = 0 then None else Some configurations
    /// Some if only one project, else None
    member x.ProjectToBuild
        with get() =
            let ps = x.ProjectsToBuild
            if ps.IsNone then None
            else if ps.Value.Length = 1 then Some ps.Value.[0]
            else None
    /// Some if only one platform configuration, else None
    member x.PlatformConfiguration
        with get() =
            let pcs = x.PlatformConfigurations
            if pcs.IsNone then None
            else if pcs.Value.Length = 1 then Some pcs.Value.[0]
            else None
    member x.TestSpecs 
        with get() = match x.Get "TestSpecs" with | Some o -> o :?> TestSpecList |> Some | None -> None
        and set (value:option<TestSpecList>) = match value with | Some v -> x.["TestSpecs"] <- v | None -> x.Remove "TestSpecs"
    member x.CleanWorkspace 
        with get() = match x.Get "CleanWorkspace" with | Some o -> o :?> CleanWorkspaceOption |> Some | None -> None
        and set (value:option<CleanWorkspaceOption>) = match value with | Some v -> x.["CleanWorkspace"] <- v | None -> x.Remove "CleanWorkspace"
    member x.RunCodeAnalysis 
        with get() = match x.Get "RunCodeAnalysis" with | Some o -> o :?> CodeAnalysisOption |> Some | None -> None
        and set (value:option<CodeAnalysisOption>) = match value with | Some v -> x.["RunCodeAnalysis"] <- v | None -> x.Remove "RunCodeAnalysis"
    member x.SourceAndSymbolServerSettings 
        with get() = match x.Get "SourceAndSymbolServerSettings" with | Some o -> o :?> SourceAndSymbolServerSettings |> Some | None -> None
        and set (value:option<SourceAndSymbolServerSettings>) = match value with | Some v -> x.["SourceAndSymbolServerSettings"] <- v | None -> x.Remove "SourceAndSymbolServerSettings"
    member x.CreateWorkItem 
        with get() = match x.Get "CreateWorkItem" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CreateWorkItem"] <- v | None -> x.Remove "CreateWorkItem"
    member x.PerformTestImpactAnalysis 
        with get() = match x.Get "PerformTestImpactAnalysis" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["PerformTestImpactAnalysis"] <- v | None -> x.Remove "PerformTestImpactAnalysis"
    member x.CreateLabel 
        with get() = match x.Get "CreateLabel" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CreateLabel"] <- v | None -> x.Remove "CreateLabel"
    member x.DisableTests 
        with get() = match x.Get "DisableTests" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["DisableTests"] <- v | None -> x.Remove "DisableTests"

    // Git
    member x.SolutionToBuild 
        with get() = match x.Get "SolutionToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["SolutionToBuild"] <- v | None -> x.Remove "SolutionToBuild"
    member x.ConfigurationToBuild 
        with get() = match x.Get "ConfigurationToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["ConfigurationToBuild"] <- v | None -> x.Remove "ConfigurationToBuild"
    member x.PlatformToBuild 
        with get() = match x.Get "PlatformToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["PlatformToBuild"] <- v | None -> x.Remove "PlatformToBuild"
    member x.TestSpec 
        with get() = match x.Get "TestSpec" with | Some o -> o :?> AgileTestPlatformSpec |> Some | None -> None
        and set (value:option<AgileTestPlatformSpec>) = match value with | Some v -> x.["TestSpec"] <- v | None -> x.Remove "TestSpec"
    member x.CleanRepository 
        with get() = match x.Get "CleanRepository" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CleanRepository"] <- v | None -> x.Remove "CleanRepository"

type IBuildDefinition with
    member x.Parameters
        with get() = TfsProcessParameters(x.ProcessParameters |> WorkflowHelpers.DeserializeProcessParameters)
        and set (v:TfsProcessParameters) = x.ProcessParameters <- WorkflowHelpers.SerializeProcessParameters v.Dictionary

type VersionControlServer with
    member x.CreateLocalWorkspace name =
        let wsp = CreateWorkspaceParameters name
        wsp.Location <- Nullable WorkspaceLocation.Local
        x.CreateWorkspace wsp
    member x.GetWorkspaceByName name =
        let owner = System.Security.Principal.WindowsIdentity.GetCurrent().Name
        x.GetWorkspace(name, owner)
    member x.DeleteWorkspaceByName name =
        let owner = System.Security.Principal.WindowsIdentity.GetCurrent().Name
        x.DeleteWorkspace(name, owner)

/// wraps a project collection and team project
type TfsProject(uri:string) =
    let tfs, project =
        let uri = Uri uri
        let path = uri.AbsolutePath.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)
        if path.Length < 2 then failwithf "invalid team project uri: %s" uri.AbsoluteUri
        let pc = Text.StringBuilder()
        pc.Appendf "%s://%s" uri.Scheme uri.Authority
        for i in 0 .. path.Length-2 do
            pc.Appendf "/%s" path.[i]
        let tfs = new Client.TfsTeamProjectCollection(Uri pc.String)
        tfs, path.[path.Length-1]
    let bs = tfs.BuildServer

    member x.FoundationRegistry with get() = tfs.FoundationRegistry
    member x.IdentityManagementService with get() = tfs.IdentityManagementService
    member x.JobService with get() = tfs.JobService
    member x.PropertyService with get() = tfs.PropertyService
    member x.EventService with get() = tfs.EventService
    member x.SecurityService with get() = tfs.SecurityService
    member x.LocationService with get() = tfs.LocationService
    member x.HyperlinkService with get() = tfs.HyperlinkService
    member x.AdministrationService with get() = tfs.AdministrationService
    member x.VersionControlServer with get() = tfs.VersionControlServer
    member x.WorkItemStore with get() = tfs.WorkItemStore
    member x.BuildServer with get() = tfs.BuildServer
    member x.TestManagementService with get() = tfs.TestManagementService
    member x.Linking with get() = tfs.Linking
    member x.CommonStructureService3 with get() = tfs.CommonStructureService3
    member x.ServerStatusService with get() = tfs.ServerStatusService
    member x.ProcessTemplates with get() = tfs.ProcessTemplates

    member x.Tfs with get() = tfs
    member x.Project with get() = project
    member x.GetBuildDefinitions() = bs.QueryBuildDefinitions project
    member x.CreateBuildDefinition() = bs.CreateBuildDefinition project
    member x.GetBuildDefinition name =
        let spec = bs.CreateBuildDefinitionSpec(project, name) 
        let bds = bs.QueryBuildDefinitions(spec).Definitions
        if bds.Length = 0 then None
        else bds.[0] |> Some
    member x.GetProcessTemplates() = bs.QueryProcessTemplates project
    member x.GetProcessTemplate serverPath = bs.GetProcessTemplate project serverPath

    member x.Dispose() =
        use tfs = tfs
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()

