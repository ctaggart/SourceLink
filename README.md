
# SourceLink

<img src="http://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
SourceLink is a .NET library that automates [source indexing](http://msdn.microsoft.com/en-us/library/windows/hardware/ff556898.aspx). It enables the source control management system to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by indexing the pdb files with https links to the SCM. Access to the source code is controlled by the SCM.

In April of 2011, Buck Hodges [stated](http://blogs.msdn.com/b/buckh/archive/2011/04/11/making-debugging-easier-source-indexing-and-symbol-server.aspx) that "we are starting to have more and more tools that need access to the symbol file information and the original source code that was used for compilation". Visual Studio Debugging, Remote Debugger, IntelliTrace, Visual Studio Profiler, and WinDBG all use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to obtain the source. It "supports HTTPS sites protected by smartcards, certificates, and regular logins and passwords."

### Better than Traditional TFS Source Indexing
SourceLink can be used to source index TFS Git code and TFS TFVC code. TFS 2013 [does not yet support source indexing TFS Git code](http://msdn.microsoft.com/en-us/library/vstudio/ms181368.aspx#tfvc_or_git_details). SourceLink provides a solution for doing so. Traditional TFS TFVC source indexing requires tf.exe to be run each time a source file is needed. Using web links for source indexing is better than having to run an executable to obtain the source.

### Great for .NET Open Source 
SourceLink is perfect for open source .NET projects. If you source index your pdb files using SourceLink and ship them in the NuGet packages, users of your project will be able to download the exact source on demand while debugging. SourceLink works with GitHub, Bitbucket, Google Project Hosting, and CodePlex. Git on CodePlex is the only option known not to work yet, so [vote here](https://codeplex.codeplex.com/workitem/26806).

### How it Works
SourceLink will create a text file next to the .pdb with an extension of .pdb.srcsrv and then run pdbstr.exe to do the source indexing. pdbstr.exe ships with TFS and [Debugging Tools for Windows](http://msdn.microsoft.com/en-us/windows/hardware/hh852365.aspx).

### How to Use
[SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) is a tools only NuGet package. It is an add-on for [FAKE - F# Make](http://fsharp.github.io/FAKE/). Examples:
* blog post [Source Link to CodePlex](http://blog.ctaggart.com/2014/01/source-link-to-codeplex.html)
* SourceLink [build.fsx](https://github.com/ctaggart/SourceLink/blob/master/build.fsx)
* FSharp.Data [build.fsx](https://github.com/fsharp/FSharp.Data/blob/master/build.fsx)

### Blog
Please see my blog posts tagged with SourceLink for more details.
http://blog.ctaggart.com/search/label/SourceLink

### Run FAKE from TFS or Visual Studio Online
FAKE builds can be integrated with several build servers. SourceLink.Fake enables integration with TFS and [Visual Studio Online](http://www.visualstudio.com/). You can [code your TFS builds in F# instead of XAML](http://blog.ctaggart.com/2014/01/code-your-tfs-builds-in-f-instead-of.html). A helper library is provided that gives easy access to the entire TFS API from F# and F# Interactive.

### Issues
* Use stackoverflow with a [sourcelink](http://stackoverflow.com/questions/tagged/sourcelink) tag for questions.
* Bugs can be logged in the GitHub [issue tracker](https://github.com/ctaggart/SourceLink/issues).

### Releases

* 2014-01 [SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) 0.3.x
  * FAKE integration
  * TFS integration
  * pdbstr.exe used instead of modifying pdb directly
  * SourceLink.Build put aside for now in favor of SourceLink.Fake
* 2013-07 [SourceLink.Build](http://www.nuget.org/packages/SourceLink.Build) 0.2.x
  * SourceLink distributed using NuGet 2.5 MSBuild Targets
  * verify Git checksums using [LibGit2Sharp](http://libgit2.github.com/)