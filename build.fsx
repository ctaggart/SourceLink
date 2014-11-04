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

Target "Clean" (fun _ -> !! "**/bin/" ++ "**/obj/" |> CleanDirs)

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
)

Target "Build" (fun _ ->
    !! "SourceLink.sln" |> MSBuildRelease "" "Rebuild" |> ignore
)

Target "SourceLink" (fun _ ->
    !! "Tfs/Tfs.fsproj" 
    ++ "SourceLink/SourceLink.fsproj"
    ++ "Git/Git.fsproj"
    |> Seq.iter (fun f ->
        use repo = new GitRepo(__SOURCE_DIRECTORY__)
        let proj = VsProj.LoadRelease f
        logfn "source indexing %s" proj.OutputFilePdb
        let files = proj.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        proj.VerifyPdbChecksums files
        proj.CreateSrcSrv "https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%" repo.Revision (repo.Paths files)
        Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
    )
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
            Dependencies = ["SourceLink", sprintf "[%s]" buildVersion] // exact version
        }]
    }) "Tfs/Tfs.nuspec"

//    NuGet (fun p -> 
//    { p with
//        Version = buildVersion
//        WorkingDir = "Build/bin/Release"
//        OutputPath = bin
//    }) "Build/Build.nuspec"

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
)

"Clean"
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isAppVeyorBuild || (isMono = false && hasBuildParam "link"))
    ==> "NuGet"

RunTargetOrDefault "NuGet"