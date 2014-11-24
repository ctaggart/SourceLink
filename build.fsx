#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink
open Fake.AppVeyor

let buildDate =
    let pst = TimeZoneInfo.FindSystemTimeZoneById "Pacific Standard Time"
    DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pst), pst.BaseUtcOffset)

let cfg = getBuildConfig __SOURCE_DIRECTORY__
let revision =
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    repo.Revision

type AppVeyorEnvironment with
    static member IsRepoTag = environVar "APPVEYOR_REPO_TAG" = "True"

let isAppVeyorBuild = buildServer = BuildServer.AppVeyor
let hasRepoVersionTag = isAppVeyorBuild && AppVeyorEnvironment.IsRepoTag && AppVeyorEnvironment.RepoBranch.StartsWith "v"

let release = ReleaseNotesHelper.LoadReleaseNotes "RELEASE_NOTES.md"

let versionAssembly =
    if hasRepoVersionTag then AppVeyor.AppVeyorEnvironment.RepoBranch.Substring 1
    else release.NugetVersion

let buildVersion =
    if hasRepoVersionTag then versionAssembly
    else sprintf "%s-ci%s" versionAssembly (buildDate.ToString "yyMMddHHmm") // 20 char limit

MSBuildDefaults <- { MSBuildDefaults with Verbosity = Some MSBuildVerbosity.Minimal }

Target "Clean" (fun _ -> !! "**/bin/" ++ "**/obj/" ++ "**/docs/output/" |> CleanDirs)

Target "BuildVersion" (fun _ ->
    let args = sprintf "UpdateBuild -Version \"%s\"" buildVersion
    Shell.Exec("appveyor", args) |> ignore
)

Target "AssemblyInfo" (fun _ ->
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
)

Target "Build" (fun _ ->
    !! "SourceLink.sln" |> MSBuildRelease "" "Rebuild" |> ignore
)

Target "SourceLink" (fun _ ->
    printfn "starting SourceLink"
    let sourceIndex proj pdb =
        use repo = new GitRepo(__SOURCE_DIRECTORY__)
//        let p = VsProj.LoadRelease proj // #50
        let p = VsProj.Load proj ["Configuration","Release"; "VisualStudioVersion","12.0"]
        let pdbToIndex = if Option.isSome pdb then pdb.Value else p.OutputFilePdb
        logfn "source indexing %s" pdbToIndex
        let files = p.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        p.VerifyPdbChecksums files
        p.CreateSrcSrv "https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%" repo.Revision (repo.Paths files)
        Pdbstr.exec pdbToIndex p.OutputFilePdbSrcSrv
    sourceIndex "Tfs/Tfs.fsproj" None 
    sourceIndex "SourceLink/SourceLink.fsproj" None
    sourceIndex "Git/Git.fsproj" None
    sourceIndex "SymbolStore/SymbolStore.fsproj" None
    sourceIndex "CorSym/CorSym.csproj" (Some "SymbolStore/bin/Release/SourceLink.SymbolStore.CorSym.pdb")
    sourceIndex "Exe/Exe.fsproj" None
)

Target "NuGet" (fun _ ->
    let bin = "bin"
    Directory.CreateDirectory bin |> ignore

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "SourceLink/bin/Release"
        OutputPath = bin
    }) "SourceLink/SourceLink.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "Tfs/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies = ["SourceLink.Core", sprintf "[%s]" buildVersion] // exact version
        }]
    }) "Tfs/Tfs.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "Build/bin/Release"
        OutputPath = bin
    }) "Build/Build.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "Fake"
        OutputPath = bin
    }) "Fake/Fake.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "Git/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies = ["LibGit2Sharp", GetPackageVersion "./packages/" "LibGit2Sharp"]
        }]
    }) "Git/Git.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "SymbolStore/bin/Release"
        OutputPath = bin
    }) "SymbolStore/SymbolStore.nuspec"

    NuGet (fun p -> 
    { p with
        Version = buildVersion
        WorkingDir = "Exe/bin/Release"
        OutputPath = bin
    }) "Exe/Exe.nuspec"
)

// --------------------------------------------------------------------------------------
// Generate the documentation

//Target "GenerateReferenceDocs" (fun _ ->
//    if not <| executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:REFERENCE"] [] then
//      failwith "generating reference documentation failed"
//)

let generateDocs fail =
    if executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:HELP"] [] then
        traceImportant "Help generated"
    else
        if fail then
            failwith "generating help documentation failed"
        else
            traceImportant "generating help documentation failed"
    

Target "Docs" (fun _ ->
    DeleteFile "docs/content/release-notes.md"    
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

//    DeleteFile "docs/content/license.md"
//    CopyFile "docs/content/" "LICENSE.txt"
//    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateDocs true
    CopyFile "docs/output" "SourceLink128.jpg" // icon used by all NuGet packages
)


Target "DocsRun" (fun _ ->    
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
)

// --------------------------------------------------------------------------------------

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isAppVeyorBuild || (isMono = false && hasBuildParam "link"))
    ==> "NuGet"

RunTargetOrDefault "NuGet"