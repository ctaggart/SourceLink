
# SourceLink

<img src="http://ctaggart.github.io/SourceLink/SourceLink128.jpg" align="right">
[SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) is a tools only NuGet package. It allows [FAKE - F# Make](http://fsharp.github.io/FAKE/) build scripts to do source linking. Source linking is source indexing, but only with http or https links. In April of 2011, Buck Hodges [stated](http://blogs.msdn.com/b/buckh/archive/2011/04/11/making-debugging-easier-source-indexing-and-symbol-server.aspx) that "we are starting to have more and more tools that need access to the symbol file information and the original source code that was used for compilation". Visual Studio Debugging, Remote Debugger, IntelliTrace, Visual Studio Profiler, WinDBG all use [SymSrv.dll](http://msdn.microsoft.com/en-us/library/windows/desktop/ms681416(v=vs.85).aspx) to obtain the source. "SymSrv can obtain symbol files from an HTTP or HTTPS source using the logon information provided by the operating system. SymSrv supports HTTPS sites protected by smartcards, certificates, and regular logins and passwords." This works with both private and public repositories. It works with everything from your TFS server within your corporate intranet to your open source project on GitHub. CodePlex is the only major code hosting site to not yet support raw downloads, so please [vote here](https://codeplex.codeplex.com/workitem/26806).

Packaged with SourceLink.Fake is a SourceLink.Tfs library. This libary allows:
* easy access to TFS or [Visual Studio Online](http://www.visualstudio.com/) from F# Interactive and build scripts
* TFS Activities for running FAKE from TFS or VSO Build
* ability to use the Git library LibGit2Sharp from F# Interactive and build scripts

Please see my blog posts tagged with SourceLink for more details.
http://blog.ctaggart.com/search/label/SourceLink

### Issues
* Use stackoverflow with a [sourcelink](http://stackoverflow.com/questions/tagged/sourcelink) tag for questions.
* Bugs can be logged in the GitHub [issue tracker](https://github.com/ctaggart/SourceLink/issues).

### Releases

* 2014-01 SourceLink 0.3.0 is in prerelease
  * new [SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake) package  
  * new SourceLink.Tfs package
* 2013-07-17 SourceLink.Build 0.2.1 [issues](https://github.com/ctaggart/SourceLink/issues?milestone=1&state=closed)  
  * fixed some relative path issues
* 2013-07-15 [SourceLink.Build](http://www.nuget.org/packages/SourceLink.Build) 0.2  
  * MSBuild task works with git

