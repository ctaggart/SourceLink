#I "packages/FAKE/tools"
#r "FakeLib.dll"
#load "packages/SourceLink.Fake/tools/SourceLink.fsx"

open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open SourceLink
open Fake.AppVeyor
open Fake.Testing
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
    let result =
        ExecProcess (fun info ->
            info.FileName <- @"C:\Program Files\AppVeyor\BuildAgent\appveyor.exe"
            info.Arguments <- sprintf "UpdateBuild -Version \"%s\"" buildVersion) TimeSpan.MaxValue
    if result <> 0 then failwithf "Error setting BuildVersion"

Target "AssemblyInfo" <| fun _ ->
    // let iv = Text.StringBuilder() // json
    // iv.Appendf "{\\\"buildVersion\\\":\\\"%s\\\"" buildVersion
    // iv.Appendf ",\\\"buildDate\\\":\\\"%s\\\"" (buildDate.ToString "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz")
    // if isAppVeyorBuild then
    //     iv.Appendf ",\\\"gitCommit\\\":\\\"%s\\\"" AppVeyor.AppVeyorEnvironment.RepoCommit
    //     iv.Appendf ",\\\"gitBranch\\\":\\\"%s\\\"" AppVeyor.AppVeyorEnvironment.RepoBranch
    // iv.Appendf "}"
    // let common = [ 
    //     Attribute.Version versionAssembly 
    //     Attribute.InformationalVersion iv.String ]
    // common |> CreateFSharpAssemblyInfo "SourceLink/AssemblyInfo.fs"
    () |> ignore

Target "Build" <| fun _ ->
    !! "SourceLink.sln" |> MSBuildRelease "" "Rebuild" |> ignore

Target "UnitTest" <| fun _ ->
    CreateDir "bin"
    xUnit2 (fun p -> 
        { p with
//            IncludeTraits = ["Kind", "Unit"]
            XmlOutputPath = Some @"bin\UnitTest.xml"
            Parallel = ParallelMode.All
        })
        [   @"UnitTest\bin\Release\SourceLink.UnitTest.dll" ]

let bin = "bin"
let nugetApiKey = environVarOrDefault "NuGetApiKey" ""
let chocolateyApiKey = environVarOrDefault "ChocolateyApiKey" ""
let githubToken = environVarOrDefault "GitHubToken" ""

Target "NuGet" <| fun _ ->
    Directory.CreateDirectory bin |> ignore
    // NuGet pSourceLink "SourceLink/SourceLink.nuspec"

Target "Publish" <| fun _ ->
    // NuGetPublish pSourceLink 
    () |> ignore

Target "Docs" <| fun _ ->
    DeleteFile "docs/content/release-notes.md"    
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"
    if executeFSIWithArgs "docs/tools" "generate.fsx" ["--define:RELEASE"; "--define:HELP"] [] then
        traceImportant "Help generated"
    else
        failwith "generating help documentation failed"
    CopyFile "docs/output" "SourceLink128.jpg" // icon used by all NuGet packages

Target "PushDocs" <| fun _ ->
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
==> "UnitTest"
==> "NuGet"
==> "Publish"

RunTargetOrDefault "Help"