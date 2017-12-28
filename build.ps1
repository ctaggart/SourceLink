$version = '2.7.2' # the version under development, update after a release
$versionSuffix = '-a125' # manually incremented for local builds

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

$pack = "pack", "-c", "release", "-o", "../bin", "/p:Version=$version$versionSuffix", "/v:m"
$pack += "/p:ci=true"

Set-Location $psscriptroot\dotnet-sourcelink
dotnet restore
dotnet $pack

Set-Location $psscriptroot\dotnet-sourcelink-git
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.GitHub
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.GitLab
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.BitBucket
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.BitBucketServer
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Create.CommandLine
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Test
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Embed.AllSourceFiles
dotnet restore
dotnet $pack

Set-Location $psscriptroot\SourceLink.Embed.PaketFiles
dotnet restore
dotnet $pack

Set-Location $psscriptroot\build
dotnet restore
$nupkgs = ls ..\bin\*$version$versionSuffix.nupkg
foreach($nupkg in $nupkgs){
    echo "test $nupkg"
    dotnet sourcelink test $nupkg
}

Set-Location $psscriptroot
