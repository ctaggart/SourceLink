$version = '2.0.0' # the version under development, update after a release
$versionSuffix = '-a088' # manually incremented for local builds

function isVersionTag($tag){
    $v = New-Object Version
    [Version]::TryParse($tag, [ref]$v)
}

if ($env:appveyor){
    $versionSuffix = '-b' + [int]::Parse($env:appveyor_build_number).ToString('000')
    if ($env:appveyor_repo_tag -eq 'true' -and (isVersionTag($env:appveyor_repo_tag_name))){
        $version = $env:appveyor_repo_tag_name
        $versionSuffix = ''
    }
    Update-AppveyorBuild -Version "$version$versionSuffix"
}

$pack = "pack", "-c", "release", "--include-symbols", "-o", "../bin", "/p:Version=$version$versionSuffix"

Set-Location $psscriptroot\dotnet-sourcelink
dotnet restore
dotnet $pack

Set-Location $psscriptroot\dotnet-sourcelink-git
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.GitHub
dotnet msbuild /t:Paths
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Test
dotnet restore
dotnet $pack

Set-Location $psscriptroot
bash .\build-rename.sh

# testing on local nuget feed
if (-not $env:appveyor){
    Copy-Item .\bin\*$version$versionSuffix.nupkg C:\dotnet\nupkg\
}