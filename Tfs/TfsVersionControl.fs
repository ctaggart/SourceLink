[<AutoOpen>]
module SourceLink.Tfs.TfsVersionControl

open System
open Microsoft.TeamFoundation.VersionControl.Client
open Microsoft.TeamFoundation.VersionControl.Common

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