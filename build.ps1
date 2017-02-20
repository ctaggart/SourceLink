# the version under development, update after a release
$version = '2.0.0'
$versionSuffix = '-a040' # manually incremented for local builds

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

Push-Location .\dotnet-sourcelink
dotnet restore
dotnet $pack
Pop-Location

Push-Location .\dotnet-sourcelink-git
dotnet restore
dotnet $pack
Pop-Location

Push-Location .\SourceLink.Create.GitHub
dotnet restore
dotnet $pack
Pop-Location

# testing on local nuget feed
if (-not $env:appveyor){
    bash .\build-rename.sh
    copy .\bin\*$version$versionSuffix.nupkg C:\dotnet\nupkg\
}