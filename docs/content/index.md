# SourceLink: Source Code On Demand

SourceLink allows you to download on demand the exact version of the source files that were used to build the library or application when debugging from Visual Studio or another Windows debugger. The debuggers use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to retrieve the source code. "Because the source code for a module can change between versions and over the course of years, it is important to look at the source code as it existed when the version of the module in question was built." SourceLink enables the source control management (SCM) system to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by indexing the `pdb` files with `https` links to the SCM. Access to the source code is controlled by the SCM.

### Demo: Source Code Downloads when Debugging

When you NuGet package includes the source indexed `pdb` files, users of the libraries can step directly into the source code using Visual Studio.

<iframe width="700" height="450" src="https://www.youtube.com/embed/k_jeSP_rMp8?rel=0" frameborder="0" allowfullscreen></iframe>

### Demo: Go To Source during Design Time

Visual F# Power Tools 1.8 beta uses the source indexed pdb files to get the source code during design time. In the case of GitHub, it opens your browser to the exact line.

<iframe title="YouTube video player" width="700" height="450" src="https://www.youtube.com/embed/5n3TUqMiysk?rel=0" frameborder="0" allowfullscreen></iframe>

### Typical Usage with SourceLink.exe

    SourceLink.exe index `
      -u "https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%" `
      -pr ./SourceLink/SourceLink.fsproj `
      -pp Configuration Release -pp VisualStudioVersion 12.0 `
      -nf */AssemblyInfo.fs

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

SourceLink is being used with:

  * .NET Open Source on GitHub
  * TFS 2013 On-Premise
  * GitHub private respositories soon I hope [#48](https://github.com/ctaggart/SourceLink/issues/48)