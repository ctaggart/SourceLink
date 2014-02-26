#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.Tfs.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink

let dt = DateTime.UtcNow
let cfg = getBuildConfig __SOURCE_DIRECTORY__
let revision =
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    repo.Revision
let revision8 = if revision = "" then "" else "-"+revision.Substring(0,8)

let versionAssembly = cfg.AppSettings.["versionAssembly"].Value // change when incompatible
let versionFile = cfg.AppSettings.["versionFile"].Value // matches nuget version
let prerelease =
    if hasBuildParam "prerelease" then getBuildParam "prerelease"
    else sprintf "a%s%s" (dt.ToString "yyMMddHHmm") revision8 // 20 char limit
let versionInfo = sprintf "%s %s %s" versionAssembly (dt.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") revision
let buildVersion = if String.IsNullOrEmpty prerelease then versionFile else sprintf "%s-%s" versionFile prerelease

let isAppVeyorBuild = environVar "APPVEYOR" <> null

/// http://www.appveyor.com/docs2/environment-variables
type AppVeyorEnv =
    static member ApiUrl = environVar "APPVEYOR_API_URL"
    static member ProjectId = environVar "APPVEYOR_PROJECT_ID"
    static member ProjectName = environVar "APPVEYOR_PROJECT_NAME"
    static member ProjectSlug = environVar "APPVEYOR_PROJECT_SLUG"
    static member BuildFolder = environVar "APPVEYOR_BUILD_FOLDER"
    static member BuildId = environVar "APPVEYOR_BUILD_ID"
    static member BuildNumber = environVar "APPVEYOR_BUILD_NUMBER"
    static member BuildVersion = environVar "APPVEYOR_BUILD_VERSION"
    static member JobId = environVar "APPVEYOR_JOB_ID"
    static member RepoProvider = environVar "APPVEYOR_REPO_PROVIDER"
    static member RepoScm = environVar "APPVEYOR_REPO_SCM"
    static member RepoName = environVar "APPVEYOR_REPO_NAME"
    static member RepoBranch = environVar "APPVEYOR_REPO_BRANCH"
    static member RepoCommit = environVar "APPVEYOR_REPO_COMMIT"
    static member RepoCommitAuthor = environVar "APPVEYOR_REPO_COMMIT_AUTHOR"
    static member RepoCommitAuthorEmail = environVar "APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"
    static member RepoCommitTimestamp = environVar "APPVEYOR_REPO_COMMIT_TIMESTAMP"
    static member RepoCommitMessage = environVar "APPVEYOR_REPO_COMMIT_MESSAGE"

Target "AppVeyor" (fun _ ->
    logfn "ApiUrl: %s" AppVeyorEnv.ApiUrl
    logfn "ProjectId: %s" AppVeyorEnv.ProjectId
    logfn "ProjectName: %s" AppVeyorEnv.ProjectName
    logfn "ProjectSlug: %s" AppVeyorEnv.ProjectSlug
    logfn "BuildFolder: %s" AppVeyorEnv.BuildFolder
    logfn "BuildId: %s" AppVeyorEnv.BuildId
    logfn "BuildNumber: %s" AppVeyorEnv.BuildNumber
    logfn "BuildVersion: %s" AppVeyorEnv.BuildVersion
    logfn "JobId: %s" AppVeyorEnv.JobId
    logfn "RepoProvider: %s" AppVeyorEnv.RepoProvider
    logfn "RepoScm: %s" AppVeyorEnv.RepoScm
    logfn "RepoName: %s" AppVeyorEnv.RepoName
    logfn "RepoBranch: %s" AppVeyorEnv.RepoBranch
    logfn "RepoCommit: %s" AppVeyorEnv.RepoCommit
    logfn "RepoCommitAuthor: %s" AppVeyorEnv.RepoCommitAuthor
    logfn "RepoCommitAuthorEmail: %s" AppVeyorEnv.RepoCommitAuthorEmail
    logfn "RepoCommitTimestamp: %s" AppVeyorEnv.RepoCommitTimestamp
    logfn "RepoCommitMessage: %s" AppVeyorEnv.RepoCommitMessage
)

Target "Clean" (fun _ -> 
    !! "**/bin/"
    ++ "**/obj/" 
    |> CleanDirs 
)

Target "BuildNumber" (fun _ ->
    use tb = getTfsBuild()
    tb.Build.BuildNumber <- sprintf "SourceLink.%s" buildVersion
    tb.Build.Save()
)

Target "BuildVersion" (fun _ ->
    let args = sprintf "UpdateBuild -Version %s" buildVersion
    let rv = Shell.Exec("appveyor", args)
    logfn "appveyor %s, exit code %d" args rv
)

Target "AssemblyInfo" (fun _ ->
    CreateFSharpAssemblyInfo "SourceLink/AssemblyInfo.fs"
        [ 
        Attribute.Version versionAssembly 
        Attribute.FileVersion versionFile
        Attribute.InformationalVersion versionInfo
        ]
    CreateFSharpAssemblyInfo "Build/AssemblyInfo.fs"
        [ 
        Attribute.Version versionAssembly 
        Attribute.FileVersion versionFile
        Attribute.InformationalVersion versionInfo
        ]
    CreateFSharpAssemblyInfo "Tfs/AssemblyInfo.fs"
        [ 
        Attribute.Version versionAssembly 
        Attribute.FileVersion versionFile
        Attribute.InformationalVersion versionInfo
        ]
    CreateFSharpAssemblyInfo "Git/AssemblyInfo.fs"
        [ 
        Attribute.Version versionAssembly 
        Attribute.FileVersion versionFile
        Attribute.InformationalVersion versionInfo
        ]
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
        logfn "source linking %s" proj.OutputFilePdb
        let files = proj.Compiles -- "**/AssemblyInfo.fs"
        repo.VerifyChecksums files
        proj.VerifyPdbChecksums files
        proj.CreateSrcSrv "https://raw.github.com/ctaggart/SourceLink/{0}/%var2%" repo.Revision (repo.Paths files)
        Pdbstr.exec proj.OutputFilePdb proj.OutputFilePdbSrcSrv
    )
)

Target "NuGet" (fun _ ->
    let bin = if isTfsBuild then "../bin" else "bin"
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
            Dependencies =
            [
                "SourceLink", sprintf "[%s]" buildVersion // exact version
            ]
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
)

"Clean"
    =?> ("AppVeyor", isAppVeyorBuild)
    =?> ("BuildNumber", isTfsBuild)
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isMono = false && hasBuildParam "skipSourceLink" = false)
    ==> "NuGet"

RunTargetOrDefault "NuGet"