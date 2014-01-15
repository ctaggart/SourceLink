#r "packages\FAKE.2.4.8.0\Tools\FakeLib.dll"
#load "packages\SourceLink.Tfs.0.3.0-a1401150519-89c6abbd\Fake.fsx"

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

    use fs = new StreamWriter(@"TFS\Assemblies.fsx")
    "#load \"AssembliesFramework.fsx\"" |> fs.WriteLine
    "#I __SOURCE_DIRECTORY__" |> fs.WriteLine
    sprintf "#r \"..\\..\\packages\\LibGit2Sharp.0.15.0.0\\lib\\Net35\\LibGit2Sharp.dll\"" |> fs.WriteLine
    sprintf "#r \"..\..\packages\\SourceLink.%s\\lib\\Net45\\SourceLink.dll\"" versionNuget |> fs.WriteLine
    sprintf "#r \"lib\\Net45\\SourceLink.Tfs.dll\"" |> fs.WriteLine
)

Target "Build" (fun _ ->
    !! "SourceLink.sln"
    |> MSBuildRelease "" "Rebuild"
    |> ignore
)

Target "SourceLink" (fun _ ->
    !! "Tfs\Tfs.fsproj" 
//    ++ "SourceLink\SourceLink.fsproj"
    |> Seq.iter (fun proj ->
        // verifyGitChecksums
//        logfn "verifyChecksums for %s" (Path.GetFileName proj)
//        let p = VsProject.Load proj ["Configuration","Release"]
//        let files = p.Compiles -- "**\AssemblyInfo.fs"
//        verifyChecksums repo files
//        logfn "OutputFile: %s" p.OutputFile

        // verifyPdbChecksums
//        let pdb = p.OutputFilePdb
        ()
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
    =?> ("BuildNumber", isTfsBuild)
    ==> "AssemblyInfo"
    ==> "Build"
    ==> "SourceLink"
    ==> "NuGet"

RunTargetOrDefault "NuGet"