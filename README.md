
# SourceLink
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
Source link support allows source code to be downloaded on demand while debugging. SourceLink is a set of build tools to help create and test for source link support.

Additional [documentation is on the wiki](https://github.com/ctaggart/SourceLink/wiki).

Here is the [General, Debugging, Options Dialog Box](https://docs.microsoft.com/en-us/visualstudio/debugger/general-debugging-options-dialog-box) from Visual Studio 2017:
![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

### Enable source server support
[SourceLink v1](https://github.com/ctaggart/SourceLink/wiki/SourceLink-v1) automates [source indexing](http://msdn.microsoft.com/en-us/library/windows/hardware/ff556898.aspx) of Windows PDB files. It enables the source code repostiory to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by updating the Windows PDB files with a source index of https links. Source indexing is done by modifying the Windows PDB file after a compile.

### Enable source link support
SourceLink v2 helps enable source link support using the [Portable PDB](https://github.com/dotnet/core/blob/master/Documentation/diagnostics/portable_pdb.md) format. They are cross platform and several times smaller than Windows PDB files. The implementation and specification are open source. Source link support is [in the spec](https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#SourceLink). The source link JSON file is built before the compile and the .NET compilers embeds it in the Portable PDB file. The compilers shipping with Visual Studio 2017 and with the DotNet SDKs support the `/sourcelink` option. Here is the relevant help for the latest C# compiler:
```
. "C:\Program Files\dotnet\sdk\1.0.0-rc4-004771\Roslyn\RunCsc.cmd" /?

 /debug:{full|pdbonly|portable|embedded}
                               Specify debugging type ('full' is default,
                               'portable' is a cross-platform format,
                               'embedded' is a cross-platform format embedded into
                               the target .dll or .exe)
                               
 /embed                        Embed all source files in the PDB.
 
 /embed:<file list>            Embed specific files in the PDB
 
 /sourcelink:<file>            Source link info to embed into Portable PDB.
```
We recommend using the `embedded` debug type. This works for both .NET Framework and .NET Core. If you choose to embed all source files, you don't need this tool. This may be useful to private repositories before authentication is added to the debugging tools. Hopefully, [authentication support](https://github.com/dotnet/roslyn/issues/12759#issuecomment-282793617) will be in the first update for Visual Studio 2017. 

### dotnet sourcelink

It is possible to create a source link JSON without these tools. Here is [an example](https://github.com/ctaggart/sourcelink-test/blob/18a795d827a4b9913d4d0e1f0e6ac533ab508670/src/ClassLibrary1/ClassLibrary1.csproj) just using the git command line. However, it is easy to get wrong. dotnet-sourcelink is tool you can use to test that the source link works for the Portable PDB.

Install by adding this `DotNetCliToolReference` to the project file:
``` xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard1.6</TargetFramework>
    <DebugType>embedded</DebugType>
  </PropertyGroup>
  <ItemGroup>
    <DotNetCliToolReference Include="dotnet-sourcelink" Version="2.0.2" />
  </ItemGroup>
</Project>
```

From the project folder, you can then test the Portable PDB by running `dotnet sourcelink test`. Simply run `dotnet sourcelink` for a list of other commands. Source link support and this tool are not tied to git at all.

If you wish to have `dotnet sourcelink test` run on your build server for each build, you can add the MSBuild targets by adding this to your project file too:
``` xml
<PackageReference Include="SourceLink.Test" Version="2.0.2" PrivateAssets="all" />
```
You can control when it runs by setting the `SourceLinkTest` property. It defaults to running when CI is true, so it will run automatically on continuous integration servers like AppVeyor and Travis CI which have that environment variable set. In general these tools are meant to be run only on your build server, but it is simple to test locally with:
```
dotnet build /p:ci=true /v:n
```

### dotnet sourcelink-git

The debugger will download the source file and verify that its checksum matches the one used when it was compiled. This means the source file that is compiled must be committed to the repository and the file must match exactly, including the line endings. For Windows builds, it is recommended that you configure git with `core.autocrlf input`. A git repository saves files with line endings of `lf`, but Git on Windows defaults to `core.autocrlf true`, which makes checksums not match due to the `crlf` line endings.

By default, sourcelink-git will verify that all of the source files are in the repository and that their checksums match. If the checksums do not match due to line endings, it will automatically fix them to match the git repository like endings of `lf`. If the file checksums still don't match, it will tell the compiler to embed it in the Portable PDB. If the file is not in the git repository, it will tell the compiler to embed it in the Portable PDB. All of these settings are configurable.

``` xml
<DotNetCliToolReference Include="dotnet-sourcelink-git" Version="2.0.2" />
```

The tool can be run automatically by installing MSBuild targets. This tool automatically figures out the `SourceLinkUrl` based on a git remote origin for GitHub. That property can be set manually for other providers. Contributions for other MSBuild targets for other providers are welcome.

``` xml
<PackageReference Include="SourceLink.Create.GitHub" Version="2.0.2" PrivateAssets="all" />
```
