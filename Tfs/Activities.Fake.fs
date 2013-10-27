namespace SourceLink.TfsActivities

open System
open System.IO
open System.Activities
open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.Build.Client

type Fake() = 
    inherit Process()

    // packages\FAKE.2.1.246-alpha
    // packages\FAKE.2.1.365-alpha
    /// looks in the nuget packages folder to find FAKE
    let findFake workdir =
        let dirs = Directory.EnumerateDirectories((Path.Combine(workdir, "packages")), "FAKE.*") |> Array.ofSeq
        if dirs.Length = 0 then None
        else dirs.[dirs.Length-1] |> Some

    member val BuildFsx = InArgument<string>() with get, set

    override x.Execute(context:CodeActivityContext) : unit =
        let build = context.GetExtension<IBuildDetail>()
        let fn = x.FileName.Get context
        if String.IsNullOrEmpty fn then
            let wd = x.WorkingDirectory.Get context
            let fakeDir = findFake wd
            if fakeDir = None then
                x.FailBuildWithf context "unable to find FAKE.exe"
            else
                x.FileName.Set(context, Path.Combine(wd, @"tools\FAKE.exe"))
                let buildFsx =
                    let buildFsx = x.BuildFsx.Get context
                    if String.IsNullOrEmpty buildFsx then "build.fsx" else buildFsx
                let args =
                    let args = x.Arguments.Get context
                    if String.IsNullOrEmpty buildFsx then "" else sprintf " %s" args
                let tfsProjectCollection = build.BuildServer.TeamProjectCollection.Uri.AbsoluteUri
                let tfsBuildUri = build.Uri.AbsoluteUri
                x.Arguments <- InArgument<string>(sprintf "%s tfsProjectCollection\"%s\" tfsBuildUri=\"%s\"%s" buildFsx tfsProjectCollection tfsBuildUri args)
        base.Execute context