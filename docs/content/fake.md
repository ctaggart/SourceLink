# Integrate with FAKE

[SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) is a tools only NuGet package that is an add-on for [FAKE - F# Make](http://fsharp.github.io/FAKE/). It is used by a lot of projects using FAKE and is includes as part of [ProjectScaffold](http://fsprojects.github.io/ProjectScaffold/). Here is the  target in its `build.fsx`:

```
Target "SourceLink" (fun _ ->
    let baseUrl = sprintf "%s/%s/{0}/%%var2%%" gitRaw project
    !! "src/**/*.??proj"
    |> Seq.iter (fun projFile ->
        let proj = VsProj.LoadRelease projFile
        SourceLink.Index proj.CompilesNotLinked proj.OutputFilePdb __SOURCE_DIRECTORY__ baseUrl
    )
)
```

### Octokit.NET example

```
Target "SourceLink" (fun _ ->
    [   "Octokit/Octokit.csproj"
        "Octokit/Octokit-netcore45.csproj"
        "Octokit/Octokit-Portable.csproj"
        "Octokit.Reactive/Octokit.Reactive.csproj" ]
    |> Seq.iter (fun pf ->
        let proj = VsProj.LoadRelease pf
        let url = "https://raw.githubusercontent.com/octokit/octokit.net/{0}/%var2%"
        SourceLink.Index proj.Compiles proj.OutputFilePdb __SOURCE_DIRECTORY__ url
    )
)
```

### pdbstr.exe is required

`pdbstr.exe` comes with Debugging Tools for Windows that is a part of the Windows 8.1 SDK. That SDK is preinstalled on build services like AppVeyor. It is also included with the `SourceLink` NuGet package that contains `SourceLink.exe`. If you install `SourceLink` using Chocolatey, `SourceLink.Fake` will [find it](https://github.com/ctaggart/SourceLink/blob/master/SourceLink/Pdbstr.fs#L9-L10).
