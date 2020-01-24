$version = '3.2.0' # the version under development, update after a release
$versionSuffix = '-build.0' # manually incremented for local builds

function isVersionTag($tag){
    $v = New-Object Version
    [Version]::TryParse($tag, [ref]$v)
}

if ($env:appveyor){
    $versionSuffix = '-build.' + $env:appveyor_build_number
    if ($env:appveyor_repo_tag -eq 'true' -and (isVersionTag($env:appveyor_repo_tag_name))){
        $version = $env:appveyor_repo_tag_name
        $versionSuffix = ''
    }
    Update-AppveyorBuild -Version "$version$versionSuffix"
}

# just build some, as we are not packing them yet for v3
$build = "build", "-c", "release", "/p:Version=$version$versionSuffix", "/v:m"
$pack = "pack", "-c", "release", "-o", "../bin", "/p:Version=$version$versionSuffix", "/v:m"
$pack += "/p:ci=true"

Set-Location $psscriptroot\Tests
dotnet test -c release /v:m

Set-Location $psscriptroot\dotnet-sourcelink
dotnet $pack

# Set-Location $psscriptroot\build
# dotnet restore
# $nupkgs = ls ..\bin\*$version$versionSuffix.nupkg
# foreach($nupkg in $nupkgs){
#     echo "test $nupkg"
#     dotnet sourcelink test $nupkg
# }

Set-Location $psscriptroot