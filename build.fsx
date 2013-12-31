#r @"packages\FAKE.2.4.2.0\tools\FakeLib.dll"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile

let versionAssembly = "0.3.0" // change when compatability if broken
let versionFile = "0.3.0"
let versionPre = "a" // emtpy, a for alpha, b for beta

let start = DateTime.UtcNow
let versionPreFull = if versionPre.Length = 0 then "" else sprintf "%s%s" versionPre (start.ToString "yyMMddHHmm") // pre-release, only allows 20 chars, TODO append 8 chars of 40 char git checksum hash "-a1312241626-dcc582c2"
let versionInfo = sprintf "%s %s %s" versionAssembly (start.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'") "" // TODO full git checksum
let versionNuget = if versionPreFull.Length = 0 then versionFile else sprintf "%s-%s" versionFile versionPreFull

Target "Clean" (fun _ -> 
    !! "**/bin/"
    ++ "**/obj/" 
    |> CleanDirs 
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
    "#load \"AssembliesFramework.fsx" |> fs.WriteLine
    sprintf "#r @\"packages\SourceLink.%s\lib\net45\SourceLink.dll" versionNuget |> fs.WriteLine
    sprintf "#r @\"packages\SourceLink.Tfs.%s\lib\net45\SourceLink.dll" versionNuget |> fs.WriteLine
)

Target "Build" (fun _ ->
    { BaseDirectory = __SOURCE_DIRECTORY__
      Includes = ["SourceLink.sln"]
      Excludes = [] } 
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "NuGet" (fun _ ->
    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "SourceLink/bin/Release"
        OutputPath = "SourceLink/bin/Release"
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies =
            [
                "LibGit2Sharp", GetPackageVersion "packages/" "LibGit2Sharp"
            ]
        }]
    }) "SourceLink/SourceLink.nuspec"

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "Build/bin/Release"
        OutputPath = "Build/bin/Release"
    }) "Build/Build.nuspec"

    NuGet (fun p -> 
    { p with
        Version = versionNuget
        WorkingDir = "Tfs/bin/Release"
        OutputPath = "Tfs/bin/Release"
        DependenciesByFramework =
        [{ 
            FrameworkVersion = "net45"
            Dependencies =
            [
                "SourceLink", versionNuget
            ]
        }]
    }) "Tfs/Tfs.nuspec"
)

"Clean"
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "NuGet"

RunTargetOrDefault "NuGet"