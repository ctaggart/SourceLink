#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.Tfs.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink

//type MyListener() =
//    interface ITraceListener with
//        member x.Write msg =
//            match msg with
//            | StartMessage -> printfn "StartMessage"
//            | OpenTag(tag,name) -> printfn "OpenTag %s %s" tag name
//            | CloseTag tag -> printfn "CloseTag %s" tag
//            | ImportantMessage text -> printfn "ImportantMessage %s" text
//            | ErrorMessage text -> printfn "ImportantMessage %s" text
//            | LogMessage(text,newLine) -> printfn "LogMessage %s %b" text newLine
//            | TraceMessage(text,newLine) -> printfn "TraceMessage %s %b" text newLine
//            | FinishedMessage -> printfn "FinishedMessage"

//listeners.Clear()
//listeners.Add(MyListener())
//MSBuildLoggers <- []

let dt = DateTime.UtcNow
let cfg = getBuildConfig __SOURCE_DIRECTORY__
let revision =
    use repo = new GitRepo(__SOURCE_DIRECTORY__)
    repo.Revision

let versionAssembly = cfg.AppSettings.["versionAssembly"].Value // change when incompatible
let versionFile = cfg.AppSettings.["versionFile"].Value // matches nuget version
let prerelease =
    if hasBuildParam "prerelease" then getBuildParam "prerelease"
    else sprintf "ci%s" (dt.ToString "yyMMddHHmm") // 20 char limit
let versionInfo = sprintf "%s %s %s" versionAssembly dt.IsoDateTime revision
let buildVersion = if String.IsNullOrEmpty prerelease then versionFile else sprintf "%s-%s" versionFile prerelease

Target "Clean" (fun _ -> !! "**/bin/" ++ "**/obj/" |> CleanDirs)

Target "BuildVersion" (fun _ ->
    let args = sprintf "UpdateBuild -Version \"%s\"" buildVersion
    Shell.Exec("appveyor", args) |> ignore

    if isTfsBuild then
        let tb = getTfsBuild()
        for n in tb.Build.Information.Nodes do
            logfn "node id: %d %A" n.Id n

)

Target "AssemblyInfo" (fun _ ->
    let common = [ 
        Attribute.Version versionAssembly 
        Attribute.FileVersion versionFile
        Attribute.InformationalVersion versionInfo ]
    common |> CreateFSharpAssemblyInfo "SourceLink/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Build/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Tfs/AssemblyInfo.fs"
    common |> CreateFSharpAssemblyInfo "Git/AssemblyInfo.fs"
)

Target "Build" (fun _ ->
//    !! "SourceLink.sln" |> MSBuildRelease "" "Rebuild" |> ignore
    MSBuildHelper.build  (fun p ->
    { p with
        Targets = ["Rebuild"]
        Properties = ["Configuration","Release"]
//      NoConsoleLogger = true
    }) "SourceLink.sln"
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
    =?> ("BuildVersion", buildServer = BuildServer.AppVeyor)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isMono = false && hasBuildParam "skipSourceLink" = false)
    ==> "NuGet"

RunTargetOrDefault "NuGet"