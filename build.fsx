
#r @"packages\FAKE.2.1.247-alpha\tools\FakeLib.dll"

//Microsoft.TeamFoundation.Client
// C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\ReferenceAssemblies\v2.0\Microsoft.TeamFoundation.Client.dll

open Fake

let buildDir  = "bin" //@".\bin\"

let sln = !+ @"SourceLink.sln" |> Scan

Target "Clean" (fun _ -> 
    CleanDirs [buildDir]
)

Target "Build" (fun _ ->
    MSBuildRelease buildDir "rebuild" sln |> Log ""
    if hasBuildParam "windir" then
        sprintf "windir: %s" (getBuildParam "windir") |> trace
    else
        "windir not set" |> trace
)
 
"Build" <== ["Clean"]

//Run "Default"
Run "Build"
//Run "Clean"