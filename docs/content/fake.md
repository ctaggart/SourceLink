# Integrate with FAKE

[SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) is a tools only NuGet package that is an add-on for [FAKE - F# Make](http://fsharp.github.io/FAKE/). It is used by a lot of projects using FAKE and is includes as part of [ProjectScaffold](http://fsprojects.github.io/ProjectScaffold/).

### Typical Usage with FAKE Builds
The [SourceLink.Fake](https://www.nuget.org/packages/SourceLink.Fake) NuGet package is typically used with a [FAKE](http://fsharp.github.io/FAKE/) target named `SourceLink`:

    Target "SourceLink" (fun _ ->
        let rawUrl = "https://raw.githubusercontent.com/octokit/octokit.net"
        use repo = new GitRepo(__SOURCE_DIRECTORY__)
        [   "Octokit/Octokit.csproj"
            "Octokit/Octokit-netcore45.csproj"
            "Octokit/Octokit-Portable.csproj"
            "Octokit.Reactive/Octokit.Reactive.csproj" ]
        |> Seq.iter (fun pf ->
            let proj = VsProj.LoadRelease pf
            logfn "source linking %s" proj.OutputFilePdb
            let files = (proj.Compiles -- "SolutionInfo.cs").SetBaseDirectory __SOURCE_DIRECTORY__
            repo.VerifyChecksums files
            proj.VerifyPdbChecksums files
            proj.CreateSrcSrv "%s/{0}/%var2%" rawUrl repo.Commit (repo.Paths files)
            Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
        )
    )

### pdbstr.exe is required

`pdbstr.exe` comes with Debugging Tools for Windows that is a part of the Windows 8.1 SDK. It is also included with the `SourceLink` NuGet package that contains `SourceLink.exe`. If you install `SourceLink` using Chocolatey, `SourceLink.Fake` will [find it](https://github.com/ctaggart/SourceLink/blob/master/SourceLink/Pdbstr.fs#L9-L10). An alternative is to add `SourceLink` as a depdencency in Paket `paket.dependencies` or a NuGet `packages.config` and locate the path using FAKE. The path can be passed in as the last argument to `Pdbstr.execWith`.

### Examples

* blog post [Source Link to CodePlex](http://blog.ctaggart.com/2014/01/source-link-to-codeplex.html)
* SourceLink [build.fsx](https://github.com/ctaggart/SourceLink/blob/master/build.fsx)
* FSharp.Data [build.fsx](https://github.com/fsharp/FSharp.Data/blob/master/build.fsx)