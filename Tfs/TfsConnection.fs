[<AutoOpen>]
module SourceLink.TfsConnection

open Microsoft.TeamFoundation
open Microsoft.TeamFoundation.Client // requires adding references to System.Drawing and System.Windows.Forms
open Microsoft.TeamFoundation.Framework.Client
open Microsoft.TeamFoundation.Framework.Common

// abbreviations
type Tfs = TfsTeamProjectCollection
type TfsConfig = TfsConfigurationServer

// http://msdn.microsoft.com/en-us/library/bb286958.aspx
type TfsConnection with
    member x.Registry with get() = x.GetService<ITeamFoundationRegistry>()
    member x.IdentityManagementService with get() = x.GetService<IIdentityManagementService>()
    member x.JobService with get() = x.GetService<ITeamFoundationJobService>()
    member x.PropertyService with get() = x.GetService<IPropertyService>()
    member x.EventService with get() = x.GetService<IEventService>()
    member x.SecurityService with get() = x.GetService<ISecurityService>()
    member x.LocationService with get() = x.GetService<ILocationService>()
    member x.HyperlinkService with get() = x.GetService<TswaClientHyperlinkService>()
    member x.AdministrationService with get() = x.GetService<IAdministrationService>()

type TfsConfigurationServer with
    member x.ProjectCollectionService with get() = x.GetService<ITeamProjectCollectionService>()
    member x.CatalogService with get() = x.GetService<ICatalogService>()

    // requires permission: edit instance-level information
    member x.ProjectCollections with get() = x.ProjectCollectionService.GetCollections()
        
    member x.ProjectCollectionNames
        with get() = 
            x.CatalogNode.QueryChildren([|CatalogResourceTypes.ProjectCollection|], false, CatalogQueryOptions.None)
            |> Array.ofSeq |> Array.map (fun node -> node.Resource.DisplayName)

type TfsTeamProjectCollection with
    member x.VersionControlServer with get() = x.GetService<VersionControl.Client.VersionControlServer>()
    member x.WorkItemStore with get() = x.GetService<WorkItemTracking.Client.WorkItemStore>()
    member x.BuildServer with get() = x.GetService<Build.Client.IBuildServer>()
    member x.TestManagementService with get() = x.GetService<TestManagement.Client.ITestManagementService>()
    member x.Linking with get() = x.GetService<ILinking>()
    member x.CommonStructureService3 with get() = x.GetService<Server.ICommonStructureService3>()
    member x.ServerStatusService with get() = x.GetService<Server.IServerStatusService>()
    member x.ProcessTemplates with get() = x.GetService<Server.IProcessTemplates>()

    // not working with TF Service
    member x.ProjectNames
        with get() =
            x.VersionControlServer.GetAllTeamProjects false
            |> Array.map (fun p -> p.Name)