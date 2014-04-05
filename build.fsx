#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"

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
//let revision8 = if revision = "" then "" else "-"+revision.Substring(0,8)

let versionAssembly = cfg.AppSettings.["versionAssembly"].Value // change when incompatible
let versionFile = cfg.AppSettings.["versionFile"].Value // matches nuget version
let prerelease =
    if hasBuildParam "prerelease" then getBuildParam "prerelease"
    else sprintf "ci%s" (dt.ToString "yyMMddHHmm") // 20 char limit
let versionInfo = sprintf "%s %s %s" versionAssembly (dt.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") revision
let buildVersion = if String.IsNullOrEmpty prerelease then versionFile else sprintf "%s-%s" versionFile prerelease

let isAppVeyorBuild = environVar "APPVEYOR" <> null

Target "Clean" (fun _ -> 
    !! "**/bin/"
    ++ "**/obj/" 
    |> CleanDirs 
)

Target "BuildVersion" (fun _ ->
    let args = sprintf "UpdateBuild -Version \"%s\"" buildVersion
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
    =?> ("BuildVersion", isAppVeyorBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isMono = false && hasBuildParam "skipSourceLink" = false)
    ==> "NuGet"

RunTargetOrDefault "NuGet"