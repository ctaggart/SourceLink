
### 0.5.0 _ 2015-03
  * [SourceLink.exe](http://www.nuget.org/packages/SourceLink) also [on chocolatey](https://chocolatey.org/packages/SourceLink)
  * [SourceLink.SymbolStore](http://www.nuget.org/packages/SourceLink.Store)
  * SourceLink NuGet Package is now [SourceLink.Core](https://www.nuget.org/packages/SourceLink.Core)
  * better TFS logging with FAKE, bug #45 fixed by Chet Husk
  * bug fixed in PdFile.hasName by Jeremy Ansel, bug #61 IndexOutOfRange
  * [issues](https://github.com/ctaggart/SourceLink/issues?q=milestone%3A0.5.0)

### 0.4.2 _ 2014-11
  * [#47](https://github.com/ctaggart/SourceLink/issues/47) Pdbstr.exec should work with absolute paths

### 0.4.0 _ 2014-10
  * TFS integration
  * [issues](https://github.com/ctaggart/SourceLink/issues?q=milestone%3A0.4.0)

### 0.3.0 _ 2014-01
  * [SourceLink.Fake](http://www.nuget.org/packages/SourceLink.Fake)
  * FAKE integration
  * Visual Studio Online integration
  * pdbstr.exe used instead of modifying pdb directly
  * SourceLink.Build put aside for now in favor of SourceLink.Fake

### 0.2.0 _ 2013-07
  * [SourceLink.Build](http://www.nuget.org/packages/SourceLink.Build)
  * SourceLink distributed using NuGet 2.5 MSBuild Targets
  * verify Git checksums using [LibGit2Sharp](http://libgit2.github.com/)