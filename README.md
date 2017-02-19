
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
SourceLink is a set of dotnet and msbuild tools to automate source linking.

## SourceLink v1 vs v2

SourceLink version 1 automates [source indexing](http://msdn.microsoft.com/en-us/library/windows/hardware/ff556898.aspx) for Windows PDB files. It enables the source control management system to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by updating the pdb files with a source index of https links to the SCM. Access to the source code is controlled by the SCM.

SourceLink v1 is distributed a couple of ways:

[![SourceLink.Fake NuGet Status](http://img.shields.io/nuget/v/SourceLink.Fake.svg?style=flat)](https://www.nuget.org/packages/SourceLink.Fake/) [SourceLink.Fake](https://github.com/ctaggart/SourceLink/wiki/FAKE) is a component for the FAKE - F# Make

[![SourceLink.exe NuGet Status](http://img.shields.io/nuget/v/SourceLink.svg?style=flat)](https://www.nuget.org/packages/SourceLink/) [SourceLink.exe](https://github.com/ctaggart/SourceLink/wiki/SourceLink.exe) is an executable that may be installed using Chocolatey 

SourceLink v2 is a set of dotnet and msbuild tools to help create the source link info to embed into Portable PDB files. The compilers shipping with Visual Studio 2017 and with the dotnet SDKs support a new `/sourcelink` option. Here is the help for the latest C# compiler:
```
/sourcelink:<file>            Source link info to embed into Portable PDB.
```

SourceLine v2 is [under active development](https://github.com/ctaggart/SourceLink/milestone/16) and a prerelease will be out soon. Installation will hopefully be as simple as adding a single reference to one of the SourceLink.Create NuGet packages.

``` xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard1.4</TargetFramework>
    <DebugType>Portable</DebugType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SourceLink.Create.GitHub" Version="2.0.0-*" />
    <DotNetCliToolReference Include="dotnet-sourcelink-git" Version="2.0.0-*" />
  </ItemGroup>
</Project>
```

## Documentation
Additional [documentation is on the Wiki](https://github.com/ctaggart/SourceLink/wiki).
