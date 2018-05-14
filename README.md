
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">

Source link support allows source code to be downloaded on demand while debugging. SourceLink is a set of build tools to help create and test for source link support. [Source link support](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/source_link.md) is a developer productivity feature that allows unique information about an assembly's original source code to be embedded in its PDB during compilation.

## .NET Foundation

SourceLink is a [.NET Foundation](http://www.dotnetfoundation.org/projects) project. It [joined](http://www.dotnetfoundation.org/blog/2017/11/16/welcome-dnn-nunit-ironpython-mvvmcross-sourcelink-ilmerge-and-humanizer-to-the-net-foundation) in 2017-11.

## License

SourceLink is licensed under the [MIT license](LICENSE).

# Quick Start

![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

The [source link support documention](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/source_link.md) shows how to embed a source link file by running `git` commands. That is exactly how the [targets](SourceLink.Create.CommandLine/SourceLink.Create.CommandLine.targets) file for `SourceLink.Create.CommandLine` works. Add this `PackageReference` to each project that you wish to enable source link support for. See the wiki if you are [using Paket](https://github.com/ctaggart/SourceLink/wiki/Paket). A common way to add this for multiple projects is to use a `Directory.Build.props`:
``` xml
<Project>
  <ItemGroup>
    <PackageReference Include="SourceLink.Create.CommandLine" Version="2.8.0" PrivateAssets="All" /> 
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
As of SourceLink 2.7, the pdb files will automatically be included in your nupkg if you use `dotnet pack` or `msbuild /t:pack`. This makes the MSBuild properties `/p:IncludeSymbols=true` and `/p:IncludeSource=true` obsolete and you may safely disable those options. 

# Test

`dotnet sourcelink test` is a command you can use to test that the source link works. It makes sure all links work for every source file that is not embedded in the PDB. You can test a nupkg, a pdb, or a dll if the pdb is embedded. Run `dotnet sourcelink` for a list of other diagnostic commands and additional help.

Install by adding:
``` xml
<DotNetCliToolReference Include="dotnet-sourcelink" Version="2.8.0" />
```

# Embedding Source Files

For source files are not committed to the repository, it is helpful to embed them, so that they are available while debugging. Source files are not committed often when generated or downloaded from elsewhere. Here is an [example of specifying files to be embedded](https://github.com/fsharp/FSharp.Compiler.Service/pull/842/files#diff-5ea2a1626f193409e8b1742db0e0c22fR669).

## All Source Files

If you just want to embed all of the source files in the pdb and not use source link support, add this package:
``` xml
<PackageReference Include="SourceLink.Embed.AllSourceFiles" Version="2.8.0" PrivateAssets="all" />
```

# Documentation
Additional [documentation is on the wiki](https://github.com/ctaggart/SourceLink/wiki).

# Known Issues

- New project system does not copy PDBs from packages when targeting .NET Framework

  Add `SourceLink.Copy.PdbFiles` to your project file. See [#313](https://github.com/ctaggart/SourceLink/issues/313) for details.

``` xml
<Project>
  <ItemGroup>
    <PackageReference Include="SourceLink.Copy.PdbFiles" Version="2.8.0" PrivateAssets="All" /> 
  </ItemGroup>
</Project>
```

- Private repositories are not supported

  Visual Studio 2017 15.7 added support for sourlink to authenticated Github.com and VSTS repositories. Support for other repository hosts is TBD: [Uservoice request](https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/19107784-debugger-should-support-authentication-with-source).

- Visual Studio does not debug into embedded source files
  
  Update to Visual Studio 2017 15.5 or later. Support [was added](https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/19107733-debugger-should-support-c-compiler-embed-optio).

# Community
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).
