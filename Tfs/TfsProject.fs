namespace SourceLink.Tfs

open System
open SourceLink

/// wraps a project collection and team project
type TfsProject(tfs:Tfs, project:string) =
    let bs = tfs.BuildServer

    new(projectUri:Uri, user:TfsUser) =
        let path = projectUri.AbsolutePath.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)
        if path.Length < 2 then failwithf "invalid team project uri: %s" projectUri.AbsoluteUri
        let pc = Text.StringBuilder()
        pc.Appendf "%s://%s" projectUri.Scheme projectUri.Authority
        for i in 0 .. path.Length-2 do
            pc.Appendf "/%s" path.[i]
        let tfs = new Tfs(Uri pc.String, user.Credentials)
        new TfsProject(tfs, path.[path.Length-1])

    new(projectUri:Uri) = new TfsProject(projectUri, TfsUser.VisualStudio)

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

