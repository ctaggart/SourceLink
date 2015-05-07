#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink
open Fake.AppVeyor
open System.Collections.Generic
open Fake.Git

let buildDate =
    let pst = TimeZoneInfo.FindSystemTimeZoneById "Pacific Standard Time"
    DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pst), pst.BaseUtcOffset)

let revision =
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    repo.Commit

let isAppVeyorBuild = buildServer = BuildServer.AppVeyor
let hasRepoVersionTag = isAppVeyorBuild && AppVeyorEnvironment.RepoTag && AppVeyorEnvironment.RepoTagName.StartsWith "v"

let release = ReleaseNotesHelper.LoadReleaseNotes "RELEASE_NOTES.md"

let versionAssembly =
    if hasRepoVersionTag then AppVeyor.AppVeyorEnvironment.RepoTagName.Substring 1
    else release.NugetVersion

let buildVersion =
    if hasRepoVersionTag then versionAssembly
    else if isAppVeyorBuild then sprintf "%s-b%s" versionAssembly AppVeyorEnvironment.BuildNumber
    else sprintf "%s-a%s" versionAssembly (buildDate.ToString "yyMMddHHmm") // 20 char limit

MSBuildDefaults <- { MSBuildDefaults with Verbosity = Some MSBuildVerbosity.Minimal }

Target "Clean" <| fun _ -> !! "**/bin/" ++ "**/obj/" ++ "**/docs/output/" |> CleanDirs

Target "BuildVersion" <| fun _ ->
    let args = sprintf "UpdateBuild -Version \"%s\"" buildVersion
    Shell.Exec("appveyor", args) |> ignore

Target "AssemblyInfo" <| fun _ ->
    let iv = Text.StringBuilder() // json
    iv.Appendf "{\\\"buildVersion\\\":\\\"%s\\\"" buildVersion
    iv.Appendf ",\\\"buildDate\\\":\\\"%s\\\"" (buildDate.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz")
    if isAppVeyorBuild then
        iv.Appendf ",\\\"gitCommit\\\":\\\"%s\\\"" AppVeyor.AppVeyorEnvironment.RepoCommit
        iv.Appendf ",\\\"gitBranch\\\":\\\"%s\\\"" AppVeyor.AppVeyorEnvironment.RepoBranch
    iv.Appendf "}"
    let common = [ 
        Attribute.Version versionAssembly 
        Attribute.InformationalVersion iv.String ]
    common |> CreateFSharpAssemblyInfo "SourceLink/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Build/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Tfs/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Git/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "SymbolStore/AssemblyInfo.fs"
    common |> CreateCSharpAssemblyInfo "CorSym/Properties/AssemblyInfo.cs"
    common |> CreateFSharpAssemblyInfo "Exe/AssemblyInfo.fs"

Target "Build" <| fun _ ->
    !! "SourceLink.sln" |> MSBuildRelease "" "Rebuild" |> ignore

Target "SourceLink" <| fun _ ->
    printfn "starting SourceLink"
    let sourceIndex proj pdb =
        use repo = new GitRepo(__SOURCE_DIRECTORY__)
        let p = VsProj.LoadRelease proj
        //let p = VsProj.Load proj ["Configuration","Release"; "VisualStudioVersion","12.0"] // on AppVeyor
        let pdbToIndex = if Option.isSome pdb then pdb.Value else p.OutputFilePdb
        logfn "source indexing %s" pdbToIndex
        let files = p.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        p.VerifyPdbChecksums files
        p.CreateSrcSrv "https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%" repo.Commit (repo.Paths files)
        Pdbstr.exec pdbToIndex p.OutputFilePdbSrcSrv
    sourceIndex "Tfs/Tfs.fsproj" None 
    sourceIndex "SourceLink/SourceLink.fsproj" None
    sourceIndex "Git/Git.fsproj" None
    sourceIndex "SymbolStore/SymbolStore.fsproj" None
    sourceIndex "CorSym/CorSym.csproj" (Some "SymbolStore/bin/Release/SourceLink.SymbolStore.CorSym.pdb")
    sourceIndex "Exe/Exe.fsproj" None

let bin = "bin"
let nugetApiKey = environVarOrDefault "NuGetApiKey" ""
let chocolateyApiKey = environVarOrDefault "ChocolateyApiKey" ""
let githubToken = environVarOrDefault "GitHubToken" ""

let pSourceLink (p: NuGetParams) =
    { p with
        Project = "SourceLink.Core"
        Version = buildVersion
        WorkingDir = "SourceLink/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
            [{ 
                FrameworkVersion = "net45"
                Dependencies = [    "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core"
                                    "SourceLink.MSBuild", GetPackageVersion "./packages/" "SourceLink.MSBuild" ] 
            }]
        AccessKey = nugetApiKey
    }

let pTfs (p: NuGetParams) =
    { p with
        Project = "SourceLink.Tfs"
        Version = buildVersion
        WorkingDir = "Tfs/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
            [{ 
                FrameworkVersion = "net45"
                Dependencies = [    "SourceLink.Core", sprintf "[%s]" buildVersion // exact version
                                    "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ] 
            }]
        AccessKey = nugetApiKey
    }

let pFake (p: NuGetParams) =
    { p with
        Project = "SourceLink.Fake"
        Version = buildVersion
        WorkingDir = "Fake"
        OutputPath = bin
        AccessKey = nugetApiKey
    }

let pGit (p: NuGetParams) =
    { p with
        Project = "SourceLink.Git"
        Version = buildVersion
        WorkingDir = "Git/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
            [{ 
                FrameworkVersion = "net45"
                Dependencies = [    "LibGit2Sharp", GetPackageVersion "./packages/" "LibGit2Sharp"
                                    "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ]
            }]
        AccessKey = nugetApiKey
    }

let pSymbolStore (p: NuGetParams) =
    { p with
        Project = "SourceLink.SymbolStore"
        Version = buildVersion
        WorkingDir = "SymbolStore/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
            [{ 
                FrameworkVersion = "net45"
                Dependencies = [ "FSharp.Core", GetPackageVersion "./packages/" "FSharp.Core" ] 
            }]
        AccessKey = nugetApiKey
    }

let pExe (p: NuGetParams) =
    { p with
        Project = "SourceLink"
        Version = buildVersion
        WorkingDir = "Exe/bin/Release"
        OutputPath = bin
        AccessKey = nugetApiKey
    }

let pExeChocolatey (p: NuGetParams) =
    { pExe p with
        PublishUrl = "https://chocolatey.org/"
        AccessKey = chocolateyApiKey
    }

Target "NuGet" <| fun _ ->
    Directory.CreateDirectory bin |> ignore
    NuGet pSourceLink "SourceLink/SourceLink.nuspec"
    NuGet pTfs "Tfs/Tfs.nuspec"
    NuGet pFake "Fake/Fake.nuspec"
    NuGet pGit "Git/Git.nuspec"
    NuGet pSymbolStore "SymbolStore/SymbolStore.nuspec"
    NuGet pExe "Exe/Exe.nuspec"

Target "Publish" <| fun _ ->
    NuGetPublish pSourceLink 
    NuGetPublish pTfs
    NuGetPublish pFake
    NuGetPublish pGit
    NuGetPublish pSymbolStore
    NuGetPublish pExe
    NuGetPublish pExeChocolatey

//Target "GenerateReferenceDocs" <| fun _ ->
//    if not <| executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:REFERENCE"] [] then
//      failwith "generating reference documentation failed"

let generateDocs fail =
    if executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:HELP"] [] then
        traceImportant "Help generated"
    else
        if fail then
            failwith "generating help documentation failed"
        else
            traceImportant "generating help documentation failed"

Target "Docs" <| fun _ ->
    DeleteFile "docs/content/release-notes.md"    
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

//    DeleteFile "docs/content/license.md"
//    CopyFile "docs/content/" "LICENSE.txt"
//    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateDocs true
    CopyFile "docs/output" "SourceLink128.jpg" // icon used by all NuGet packages

Target "WatchDocs" <| fun _ ->
    use watcher = new FileSystemWatcher(DirectoryInfo("docs/content").FullName,"*.*")
    watcher.EnableRaisingEvents <- true
    watcher.Changed.Add(fun e -> generateDocs false)
    watcher.Created.Add(fun e -> generateDocs false)
    watcher.Renamed.Add(fun e -> generateDocs false)
    watcher.Deleted.Add(fun e -> generateDocs false)

    traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.EnableRaisingEvents <- false
    watcher.Dispose()

Target "UpdateDocs" <| fun _ ->
    CleanDir "gh-pages"
    let auth = if githubToken = "" then "" else sprintf "%s@" githubToken
    cloneSingleBranch "" (sprintf "https://%sgithub.com/ctaggart/SourceLink.git" auth) "gh-pages" "gh-pages"
    fullclean "gh-pages"
    CopyRecursive "docs/output" "gh-pages" true |> printfn "%A"
    StageAll "gh-pages"
    Commit "gh-pages" (sprintf "updated docs from %s" buildVersion)
    Branches.push "gh-pages"

Target "Help" <| fun _ ->
    printfn "build.cmd [<target>] [options]"
    printfn @"for FAKE help: packages\FAKE\tools\FAKE.exe --help"
    printfn "targets:"
    printfn "  * `Clean` removes temporary directories"
    printfn "  * `Build` builds the solution"
    printfn "  * `SourceLink` source indexes the built pdb files"
    printfn "  * `NuGet` creates the nupkg files"
    printfn "  * `Docs` creates the documentation"

// chain targets together only on AppVeyor
let (==>) a b = a =?> (b, isAppVeyorBuild)

"BuildVersion"
==> "AssemblyInfo"
==> "Build"
==> "SourceLink"
==> "NuGet"
==> "Publish"

"Docs"
==> "UpdateDocs"

let runTargets() =
    // when on AppVeyor, allow targets to be specified as #hashtags
    if isAppVeyorBuild then
        if hasRepoVersionTag then
            run "Publish"
        else
            let targets = getAllTargetsNames() |> (HashSet.ofSeqCmp StringComparer.OrdinalIgnoreCase)
            let cm = AppVeyorEnvironment.RepoCommitMessage
            let rx = Text.RegularExpressions.Regex @"\B#([a-zA-Z]\w+)"
            let hashtags = seq {
                for m in rx.Matches cm do
                    yield m.Groups.[1].Value } |> List.ofSeq
            if hashtags.Length = 0 then
                RunTargetOrDefault "NuGet"
            else
                for ht in hashtags do
                    if targets.Contains ht then
                        run ht
    else
        RunTargetOrDefault "Help"

runTargets()