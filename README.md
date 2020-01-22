
# Source Link
<img src="https://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">

Source Link support allows source code to be downloaded on demand while debugging. Source Link is a set of build tools to help create and test for source link support. [Source Link support](https://github.com/dotnet/designs/blob/master/accepted/diagnostics/source-link.md) is a developer productivity feature that allows unique information about an assembly's original source code to be embedded in its PDB during compilation.

![image](https://cloud.githubusercontent.com/assets/80104/23337630/001cedb6-fbba-11e6-9c44-68f4c826470c.png)

## .NET Foundation

Source Link is now a [.NET Foundation](http://www.dotnetfoundation.org/) project at https://github.com/dotnet/sourcelink/. It [joined](http://www.dotnetfoundation.org/blog/2017/11/16/welcome-dnn-nunit-ironpython-mvvmcross-sourcelink-ilmerge-and-humanizer-to-the-net-foundation) in 2017-11. [Announced for .NET Core 2.1](https://blogs.msdn.microsoft.com/dotnet/2018/05/30/announcing-net-core-2-1/), much of the Source Link support has been integrated into the SDK.

New tools are shipped in the NuGet Gallery as [Microsoft.SourceLink.*](https://www.nuget.org/packages?q=Microsoft.SourceLink.*). They work on all platforms supported by .NET Core.

Most of the tools from this repository are made obsolete by the .NET SDK 2.1 and Microsoft.SourceLink tools. See [wiki](https://github.com/ctaggart/SourceLink/wiki) for detials. This repository will still continue to fill in gaps in tooling. The only tool so far is a `sourcelink` command-line tool for testing for Source Link support.

# Source Link testing tool

[sourcelink](https://www.nuget.org/packages/SourceLink) is a [.NET Core global tool](https://natemcmaster.com/blog/2018/05/12/dotnet-global-tools/).
```
dotnet tool install --global sourcelink
```

`sourcelink` is a global tool to test that the Source Link enabled nupkg or pdb file work. It tests all source files listed in the pdb. You can also print diagnostic information for a pdb. Run `sourcelink` without any options for a list of diagnostic commands and help:

```
SourceLink 3.0.0
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

For version 2, it is just a [dotnet cli tool](https://docs.microsoft.com/en-us/dotnet/core/tools/extensibility), run as `dotnet sourcelink` and installed by adding:
``` xml
<DotNetCliToolReference Include="dotnet-sourcelink" Version="2.8.3" />
```

# License

SourceLink tools are licensed under the [MIT license](LICENSE).

# Documentation
Additional [documentation is on the wiki](https://github.com/ctaggart/SourceLink/wiki).

# Community
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).
