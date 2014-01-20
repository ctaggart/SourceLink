#r "packages/FAKE.2.4.8.0/tools/FakeLib.dll"
#load "packages/SourceLink.Fake.0.3.0-a1401160102-fc7f738e/tools/Fake.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink

let versionAssembly = "0.3.0" // change when incompatible
let versionFile = "0.3.0" // matches nuget version
let versionPre = "a" // emtpy, a for alpha, b for beta

let repo = new GitRepo(__SOURCE_DIRECTORY__)
let dt = DateTime.UtcNow
let versionInfo = sprintf "%s %s %s" versionAssembly (dt.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") repo.Revision
// pre-release limited to 20 chars after the dash, using 10 for the date and first 8 of revision "-a1312241626-dcc582c2"
let versionNuget = if versionPre.Length = 0 then versionFile else sprintf "%s-%s%s-%s" versionFile versionPre (dt.ToString "yyMMddHHmm") (repo.Revision.Substring(0,8))

Target "Clean" (fun _ -> 
    !! "**/bin/"
    ++ "**/obj/" 
    |> CleanDirs 
)

Target "BuildNumber" (fun _ -> 
    use tb = getTfsBuild()
    tb.Build.BuildNumber <- sprintf "SourceLink.%s" versionNuget
    tb.Build.Save()
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
)

Target "Build" (fun _ ->
    !! "SourceLink.sln"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "SourceLink" (fun _ ->
    !! "Tfs/Tfs.fsproj" 
    ++ "SourceLink/SourceLink.fsproj"
    |> Seq.iter (fun proj ->
        let p = VsProject.Load proj ["Configuration","Release"]
        let files = p.Compiles -- "**/AssemblyInfo.fs"
        verifyGitChecksums repo files
        verifyPdbChecksums p files
        p.SourceLink "https://raw.github.com/ctaggart/SourceLink/{0}/%var2%" repo.Revision (repo.Paths files)
        let cmd =
            if isTfsBuild then @"C:\Program Files\Microsoft Team Foundation Server 12.0\Tools\pdbstr.exe"
            else @"C:\Program Files (x86)\Windows Kits\8.0\Debuggers\x64\srcsrv\pdbstr.exe"
        let args = sprintf "-w -s:srcsrv -i:%s -p:%s" (Path.GetFileName p.OutputFilePdbSrcSrv) (Path.GetFileName p.OutputFilePdb)
        logfn "exec %s, %s, %s" cmd args p.OutputDirectory
        Shell.Exec(cmd, args, p.OutputDirectory) |> ignore
    )
)

Target "NuGet" (fun _ ->
    let bin = if isTfsBuild then "../bin" else "bin"
    Directory.CreateDirectory bin |> ignore

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "SourceLink/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies =
            [
                "LibGit2Sharp", sprintf "[%s]" (GetPackageVersion "packages/" "LibGit2Sharp") // exact version
            ]
        }]
        
    }) "SourceLink/SourceLink.nuspec"

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "Tfs/bin/Release"
        OutputPath = bin
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies =
            [
                "SourceLink", sprintf "[%s]" versionNuget // exact version
            ]
        }]
    }) "Tfs/Tfs.nuspec"

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "Build/bin/Release"
        OutputPath = bin
    }) "Build/Build.nuspec"

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "Fake"
        OutputPath = bin
    }) "Fake/Fake.nuspec"
)

"Clean"
    =?> ("BuildNumber", isTfsBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    =?> ("SourceLink", isMono = false && hasBuildParam "skipSourceLink" = false)
    ==> "NuGet"

RunTargetOrDefault "NuGet"