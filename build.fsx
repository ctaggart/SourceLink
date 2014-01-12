#r @"packages\FAKE.2.4.8.0\tools\FakeLib.dll"
#load "packages\SourceLink.Tfs.0.3.0-a1401121926\Assemblies.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink

let versionAssembly = "0.3.0" // change when compatability is broken
let versionFile = "0.3.0"
let versionPre = "a" // emtpy, a for alpha, b for beta

let start = DateTime.UtcNow
let versionPreFull = if versionPre.Length = 0 then "" else sprintf "%s%s" versionPre (start.ToString "yyMMddHHmm") // pre-release, only allows 20 chars, TODO append 8 chars of 40 char git checksum hash "-a1312241626-dcc582c2"
let versionInfo = sprintf "%s %s %s" versionAssembly (start.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") "" // TODO full git checksum
let versionNuget = if versionPreFull.Length = 0 then versionFile else sprintf "%s-%s" versionFile versionPreFull

let isTfs = hasBuildParam "tfsBuild"
let getTfsBuild() = new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")

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

    use fs = new StreamWriter(@"TFS\Assemblies.fsx")
    "#load \"AssembliesFramework.fsx\"" |> fs.WriteLine
    "#I __SOURCE_DIRECTORY__" |> fs.WriteLine
    sprintf "#r \"..\..\packages\\SourceLink.%s\\lib\\Net45\\SourceLink.dll\"" versionNuget |> fs.WriteLine
    sprintf "#r \"lib\\Net45\\SourceLink.Tfs.dll\"" |> fs.WriteLine
)

Target "Build" (fun _ ->
    !! "SourceLink.sln"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "NuGet" (fun _ ->
    let bin = if isTfs then "../bin" else "bin"
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
        WorkingDir = "Build/bin/Release"
        OutputPath = bin
    }) "Build/Build.nuspec"

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
)

"Clean"
    =?> ("BuildNumber", isTfs)
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "NuGet"

RunTargetOrDefault "NuGet"