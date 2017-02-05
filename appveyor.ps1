# the version under development, update after a release
# TODO must also set in csproj for dotnet pack to work
$version = '2.0.0'
$versionSuffix = 'b001'

function isVersionTag($tag){
    $v = New-Object Version
    [Version]::TryParse($tag, [ref]$v)
}

if ($env:appveyor){
    $versionSuffix = 'b' + [int]::Parse($env:appveyor_build_number).ToString('000')
    if ($env:appveyor_repo_tag -eq 'true' -and (isVersionTag($env:appveyor_repo_tag_name))){
        $version = $env:appveyor_repo_tag_name
        $versionSuffix = ''
    }
    Update-AppveyorBuild -Version $version
}

Push-Location .\dotnet-sourcelink
dotnet restore
if([string]::IsNullOrEmpty($versionSuffix)){
    dotnet pack -c release --include-symbols -o ../bin
} else {
    dotnet pack -c release --version-suffix $versionSuffix --include-symbols -o ../bin
}

Pop-Location