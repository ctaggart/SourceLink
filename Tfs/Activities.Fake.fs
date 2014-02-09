namespace SourceLink.Activities

open System
open System.IO
open System.Activities
open Microsoft.TeamFoundation.Build.Client
open SourceLink

type Fake() = 
    inherit CodeActivity()

    /// looks in the nuget packages folder to find FAKE, uses the latest one
    let findFake workdir =
        let dirs = Directory.EnumerateDirectories((Path.Combine(workdir, "packages")), "FAKE.*") |> Array.ofSeq
        if dirs.Length = 0 then None
        else dirs.[dirs.Length-1] |> Some

    [<RequiredArgument>]
    member val WorkingDirectory = InArgument<string>() with get, set
    member val FileName = InArgument<string>() with get, set
    member val BuildFsx = InArgument<string>() with get, set
    member val Arguments = InArgument<string>() with get, set

    override x.CacheMetadata(metadata:CodeActivityMetadata) : unit =
        base.CacheMetadata metadata
        metadata.RequireBuildDetail()
        metadata.RequireBuildAgent()

    override x.Execute(context:CodeActivityContext) : unit =
        use tb = new TfsBuild(context.BuildAgent, context.BuildDetail)

        let workdir = x.WorkingDirectory.Get context

        let filename =
            let fn = x.FileName.Get context
            if String.IsNullOrEmpty fn = false then fn
            else
                let fakeDir = findFake workdir
                if fakeDir = None then
                    "FAKE.exe" // will need to be on the PATH
                else
                    Path.Combine(fakeDir.Value, @"tools\FAKE.exe")
        
        let buildFsx =
            let buildFsx = x.BuildFsx.Get context
            if String.IsNullOrEmpty buildFsx then "build.fsx" else buildFsx
        
        let tfs = tb.Build.BuildServer.TeamProjectCollection

        let tfsUri = tfs.Uri.AbsoluteUri
        let tfsUser = tfs.ClientCredentials.Federated.TokenValue |> Text.Encoding.UTF8.GetBytes |> Hex.encode
        let tfsBuild = tb.Build.Uri.AbsoluteUri
        let tfsAgent = tb.Agent.Uri.AbsoluteUri
        let arguments =
            let args =
                let args = x.Arguments.Get context
                if String.IsNullOrEmpty args then "" else sprintf " %s" args 
            let msBuildArgs = 
                let args = tb.Parameters.MSBuildArguments
                if args.IsNone then "" else sprintf " %s" args.Value
            sprintf "%s%s%s tfsUri=\"%s\" tfsUser=\"%s\" tfsAgent=\"%s\" tfsBuild=\"%s\"" buildFsx msBuildArgs args tfsUri tfsUser tfsAgent tfsBuild 
        context.MessageNormal "%s>%s %s" workdir filename arguments

        let p = SourceLink.Process()
        p.FileName <- filename
        p.Arguments <- arguments
        
        if String.IsNullOrEmpty workdir = false then 
            p.WorkingDirectory <- workdir
        let bm s = context.MessageHigh "%s" s
        p.Stdout |> Observable.add bm
        p.Stderr |> Observable.add bm
        try
            let exit = p.Run()
            if exit <> 0 then 
                context.FailBuildWith "FAKE failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
        with
            | ex -> 
                context.FailBuildWith "FAKE failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex