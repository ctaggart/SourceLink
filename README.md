
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
SourceLink is a set of dotnet and msbuild tools to automate source linking. [Documentation is on the Wiki](https://github.com/ctaggart/SourceLink/wiki). In brief:

![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

### source server support
[SourceLink v1](https://github.com/ctaggart/SourceLink/wiki/SourceLink-v1) automates [source indexing](http://msdn.microsoft.com/en-us/library/windows/hardware/ff556898.aspx) of Windows PDB files. It enables the source control management system to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by updating the pdb files with a source index of https links to the SCM. Source indexing is done by modifying the pdb file after the compile.

### source link support
The new source link support is done with Portable PDB files which are cross platform and many times more compact. The source link json is built before the the compile and the compiler embeds it in the pdb.

SourceLink v2 is a set of DotNet and MSBuild tools to help create the source link JSON to embed into Portable PDB files. The compilers shipping with Visual Studio 2017 and with the DotNet SDKs support a new `/sourcelink` option. Here is the help for the latest C# compiler:
```
/sourcelink:<file>            Source link info to embed into Portable PDB.
```

A prerelease of SourceLine v2 is available. Add the references below to your project file and it should run automatically on a CI server. To try it out locally, you can do `dotnet build /p:ci=true /v:n`.

``` xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard1.6</TargetFramework>
    <DebugType>Portable</DebugType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SourceLink.Create.GitHub" Version="2.0.0-*" />
    <PackageReference Include="SourceLink.Test" Version="2.0.0-*" />
    <DotNetCliToolReference Include="dotnet-sourcelink-git" Version="2.0.0-*" />
    <DotNetCliToolReference Include="dotnet-sourcelink" Version="2.0.0-*" />
  </ItemGroup>
</Project>
```
