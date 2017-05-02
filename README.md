
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
Source link support allows source code to be downloaded on demand while debugging. SourceLink is a set of build tools to help create and test for source link support.

Additional [documentation is on the wiki](https://github.com/ctaggart/SourceLink/wiki).

Here is the [General, Debugging, Options Dialog Box](https://docs.microsoft.com/en-us/visualstudio/debugger/general-debugging-options-dialog-box) from Visual Studio 2017:
![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

### Enable source server support
[SourceLink v1](https://github.com/ctaggart/SourceLink/wiki/SourceLink-v1) automates [source indexing](http://msdn.microsoft.com/en-us/library/windows/hardware/ff556898.aspx) of Windows PDB files. It enables the source code repostiory to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by updating the Windows PDB files with a source index of https links. Source indexing is done by modifying the Windows PDB file after a compile.

### Enable source link support
SourceLink v2 helps enable source link support using the [Portable PDB](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/portable_pdb.md) format. They are cross platform and several times smaller than Windows PDB files. The implementation and specification are open source. Source link support [has documentation](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/source_link.md) and is [in the Portable PDB spec](https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#SourceLink). The source link JSON file is built before the compile and the .NET compilers embeds it in the Portable PDB file. The compilers shipped with Visual Studio 2017 and with the DotNet SDKs support the `/sourcelink` option. Here is the relevant help from the C# compiler:
```
. "C:\Program Files\dotnet\sdk\1.0.0\Roslyn\RunCsc.cmd" /?

 /debug:{full|pdbonly|portable|embedded}
                               Specify debugging type ('full' is default,
                               'portable' is a cross-platform format,
                               'embedded' is a cross-platform format embedded into
                               the target .dll or .exe)
                               
 /embed                        Embed all source files in the PDB.
 
 /embed:<file list>            Embed specific files in the PDB
 
 /sourcelink:<file>            Source link info to embed into Portable PDB.
```

# Quick Start

The [source link documention](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/source_link.md) shows how to embed a source link file by running `git` commands. That is exactly how the [targets](https://github.com/ctaggart/SourceLink/blob/v2/SourceLink.Create.CommandLine/SourceLink.Create.CommandLine.targets) file for `SourceLink.Create.CommandLine` works. Simply add a `PackageReference`. A common way to do with is by adding it to your projects in a `Directory.Build.props`:
``` xml
<Project>
  <ItemGroup>
    <PackageReference Include="SourceLink.Create.CommandLine" Version="2.1.1" PrivateAssets="All" /> 
  </ItemGroup>
</Project>
```

If you are building on Windows, make sure that you configure git to checkout files with [core.autocrlf input](https://github.com/ctaggart/SourceLink/wiki/Line-Endings).

You can control when it runs by setting the `SourceLinkCreate` property. That property is set to `true` by default on build servers that set `CI` or `BUILD_NUMBER` environment variables. In general these tools are meant to be run only on build servers, but it is simple to test locally by setting an MSBuild property like `/p:ci=true` or `/p:SourceLinkCreate=true`.

If you have a dotnet project, you can test locally with:
``` ps1
dotnet restore
dotnet build /p:ci=true /v:n
```
With an full framework project, you can test locally with:
``` ps1
msbuild /t:restore
msbuild /t:rebuild /p:ci=true /v:n
```

## examples
- [Rx.NET](https://github.com/ctaggart/SourceLink/issues/167#issuecomment-297423617)
- [ASP.NET MVC](https://github.com/ctaggart/SourceLink/issues/173)

# Test

`dotnet sourcelink test` is a tool you can use to test that the source link works. Source link support and this tool are not tied to git at all. It makes sure all links work for every source file in the PDB that is not embedded. You can test a nupkg, a pdb, or a dll if the pdb is embedded. Run `dotnet sourcelink` for a list of other diagnostic commands and additional help.

Install by adding:
``` xml
<DotNetCliToolReference Include="dotnet-sourcelink" Version="2.1.1" />
```

## examples
- [SourceLink build.ps1](https://github.com/ctaggart/SourceLink/blob/v2/build.ps1#L45-L51)
- [octokit.net using Cake](https://github.com/ctaggart/SourceLink/issues/174)

`dotnet sourcelink test` may also be run by using the `SourceLink.Test` MSBuild targets.
``` xml
<PackageReference Include="SourceLink.Test" Version="2.1.1" PrivateAssets="all" />
```
Just like the `SourceLinkCreate` property, you can control when it is enabled by setting the `SourceLinkTest` property.

# dotnet sourcelink-git

Please follow the quick start if you are just getting started. `SourceLink.Create.CommandLine` uses the `git` commandline by default, does not use `dotnet`, and has been easier for new users to understand.

`SourceLink.Create.GitHub` and `SourceLink.Create.BitBucket` both use `dotnet sourcelink-git`, which accesses the git information using [libgit2sharp](https://github.com/libgit2/libgit2sharp). This allows some additional features. It verifies that all of the source files are in the git repository and that their checksums match. If checksums do not match due to line endings, it will automatically fix them to match the git repository like endings of `lf`. If a source file's checksum still does not match, it will be embedded. If the source file is not in the git repository, it will be embedded. All of these settings are configurable.

``` xml
<PackageReference Include="SourceLink.Create.GitHub" Version="2.1.1" PrivateAssets="all" />
<DotNetCliToolReference Include="dotnet-sourcelink-git" Version="2.1.1" />
```

# Embedding Source Files

For source files are not committed to the repository, it is helpful to embed them, so that they are available while debugging. Source files are not committed often when generated or downloaded from elsewhere.

## All Source Files

If you just want to embed all of the source files in the pdb and not use source link, add this package:
``` xml
<PackageReference Include="SourceLink.Embed.AllSourceFiles" Version="2.1.1" PrivateAssets="all" />
```

## Paket Files

If you are using `SourceLink.Create.CommandLine` and [Paket](https://fsprojects.github.io/Paket/)'s support for including source code that is not in your repository, you can embed those files in the pdb with:
``` xml
<PackageReference Include="SourceLink.Embed.PaketFiles" Version="2.1.1" PrivateAssets="all" />
```

# Known Issues

Please vote for all of these issues:

- GitHub NuGet: [msbuild /t:Pack always creates seperate symbols package](https://github.com/NuGet/Home/issues/4142)
  
  `dotnet pack` and `msbuild /t:pack` need to support easily packaing portalbe pdb files.

- Visual Studio User Voice: [Debugger should support C# compiler '/embed' option](https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/19107733-debugger-should-support-c-compiler-embed-optio)

  The Visual Studio 2017 debugger does not currently look for embedded source files.

- Visual Studio User Voice: [Debugger should support authentication with SourceLink](https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/19107784-debugger-should-support-authentication-with-source)

   In order to use source link with private GitHub repositories and other private repositories, it needs to support authentication.