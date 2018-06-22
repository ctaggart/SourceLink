
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">

Source link support allows source code to be downloaded on demand while debugging. SourceLink is a set of build tools to help create and test for source link support. [Source link support](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/source_link.md) is a developer productivity feature that allows unique information about an assembly's original source code to be embedded in its PDB during compilation.

![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

## .NET Foundation

SourceLink is now a [.NET Foundation](http://www.dotnetfoundation.org/) project at https://github.com/dotnet/sourcelink/. It [joined](http://www.dotnetfoundation.org/blog/2017/11/16/welcome-dnn-nunit-ironpython-mvvmcross-sourcelink-ilmerge-and-humanizer-to-the-net-foundation) in 2017-11. [Announced for .NET Core 2.1](https://blogs.msdn.microsoft.com/dotnet/2018/05/30/announcing-net-core-2-1/), much of the SourceLink support has been integrated into the SDK.

New tools from https://github.com/dotnet/sourcelink/ are in beta. They are shipped in the NuGet Gallery as [Microsoft.SourceLink.*](https://www.nuget.org/packages?q=Microsoft.SourceLink.*) `1.0.0-beta*`. They currently work on Windows and Ubuntu, but will soon support the rest.

Most of the SourceLink 2 tools from this repository are made obsolete by the .NET SDK 2.1 and Microsoft.SourceLink tools. This repository will still continue to fill in gaps in tooling. SourceLink 3 tools from this repository will be tools that build on top of the .NET SDK 2.1. The first and only tool for 3.0 is a `sourcelink` command-line tool for testing for source link support.

# Test

For SourceLink version 3, [sourcelink](https://www.nuget.org/packages/SourceLink/3.0.0-build.732) is a [.NET Core global tool](https://natemcmaster.com/blog/2018/05/12/dotnet-global-tools/).
```
dotnet tool install --global sourcelink --version 3.0.0-build.732
```

`sourcelink` is a command you can use to test that the source link works. It makes sure all links work for every source file that is not embedded in the PDB. You can test a nupkg, a pdb, or a dll if the pdb is embedded. Run `sourcelink` without any options for a list of diagnostic commands and help:

```
SourceLink 3.0.0-build.732
Source Code On Demand

Usage:  [options] [command]

Options:
  -h|--help  Show help information

Commands:
  print-documents  print the documents stored in the pdb or dll
  print-json       print the Source Link JSON stored in the pdb or dll
  print-urls       print the URLs for each document based on the Source Link JSON
  test             test each URL and verify that the checksums match

Use " [command] --help" for more information about a command.
```

For SourceLink 2, it is just a [dotnet cli tool](https://docs.microsoft.com/en-us/dotnet/core/tools/extensibility), run as `dotnet sourcelink` and installed by adding:
``` xml
<DotNetCliToolReference Include="dotnet-sourcelink" Version="2.8.3" />
```

# Quick Start

Please use the [Microsoft.SourceLink.*](https://github.com/dotnet/sourcelink/) tools if you are able to.

`SourceLink.Create.CommandLine` is the most successful SourceLink 2 tool. It contains a MSBuild [targets](SourceLink.Create.CommandLine/SourceLink.Create.CommandLine.targets) file that runs `git` commands by default to figure out the source repository to link to. Add this `PackageReference` to each project that you wish to enable source link support for. See the wiki if you are [using Paket](https://github.com/ctaggart/SourceLink/wiki/Paket). A common way to add this for multiple projects is to use a `Directory.Build.props`:
``` xml
<Project>
  <ItemGroup>
    <PackageReference Include="SourceLink.Create.CommandLine" Version="2.8.3" PrivateAssets="All" /> 
  </ItemGroup>
</Project>
```

Without any additional configuration `SourceLink.Create.CommandLine` will work with GitHub and Bitbucket cloned repositories. See the wiki for [additional options](https://github.com/ctaggart/SourceLink/wiki#sourcelinkcreatecommandline).

You can control when it runs by setting the MSBuild property `/p:SourceLinkCreate=true`.

If you have a dotnet project, you can test locally with:
``` ps1
dotnet restore
dotnet build /p:SourceLinkCreate=true /v:n
```
With an full framework project, you can test locally with:
``` ps1
msbuild /t:restore
msbuild /t:rebuild /p:SourceLinkCreate=true /v:n
```

# Package PDB in nupkg
Please package the pdb files in the nupkg files. It is [strongly encouraged](https://github.com/aspnet/Universe/issues/131#issuecomment-363269268) by Microsoft to do so. This obsoletes the MSBuild properties `/p:IncludeSymbols=true` and `/p:IncludeSource=true`. You do not need the [obsolete SymbolSource](https://github.com/SymbolSource/SymbolSource#deprecated-services-and-projects).

The new project system does not copy PDBs from packages when targeting .NET Framework [#313](https://github.com/ctaggart/SourceLink/issues/313). The simplest workaround until .NET SDK 2.1.400 ships in VS 15.8 timeframe is to add this reference:

``` xml
<Project>
  <ItemGroup>
    <PackageReference Include="SourceLink.Copy.PdbFiles" Version="2.8.3" PrivateAssets="All" /> 
  </ItemGroup>
</Project>
```

# Embedding Source Files

For source files are not committed to the repository, it is helpful to embed them, so that they are available while debugging. Source files are not committed often when generated or downloaded from elsewhere. The .NET SDK allows you to set [EmbedUntrackedSources](https://github.com/dotnet/sourcelink/blob/master/docs/README.md#embeduntrackedsources).

## All Source Files

If you just want to embed all of the source files in the pdb and not use source link support, the .NET SDK allows you to set [EmbedAllSources](https://github.com/dotnet/sourcelink/blob/master/docs/README.md#embedallsources).

# Known Issues

- Visual Studio 2017 15.8 is expected to have additional [private repository support](https://github.com/ctaggart/SourceLink/issues/204)
- Visual Studio 2017 15.7 added support for source link to authenticated Github.com and VSTS repositories
- Visual Studio 2017 15.5 added support for [debugging into embedded source files]((https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/19107733-debugger-should-support-c-compiler-embed-optio))

# License

SourceLink is licensed under the [MIT license](LICENSE).

# Documentation
Additional [documentation is on the wiki](https://github.com/ctaggart/SourceLink/wiki).

# Community
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).
