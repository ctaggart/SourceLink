
# SourceLink

<img src="https://raw.github.com/ctaggart/SourceLink/master/SourceLink128.jpg" align="right">
Provides MSBuild targets for source linking. Packages are [available on NuGet](http://nuget.org/packages/SourceLink.Build/). See my 2013-07-15 blog titled ["Source Linking"](http://blog.ctaggart.com/2013/07/source-linking.html).
Other related posts are under the ["pdb"](http://blog.ctaggart.com/search/label/pdb) label.

<img src="https://raw.github.com/ctaggart/SourceLink/master/NuGet.Core-build.png">

## Using with Git

```xml
<SourceLinkRepoUrl>https://raw.github.com/ctaggart/nuget/{0}/%var2%</SourceLinkRepoUrl>
<SourceLink Condition="'$(Configuration)'=='Release'">true</SourceLink>
```

It is simple to use it with your git repository. Install the package and add a couple of properties to your project file to specify the repository URL and to enable it. The "Source Linking" blog post gives details. Here are some examples of how you can get it working with clones of these projects. I'm hoping some of these projects will begin using SourceLink.

 * [Nuget.Core](https://github.com/ctaggart/nuget/pull/1)
 * [LibGit2Sharp](https://github.com/libgit2/libgit2sharp/pull/465)  
 * [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json/pull/103)  

## History
For details take a look at the [commits](https://github.com/ctaggart/SourceLink/commits/master).

2013-07-17 SourceLink.Build 0.2.1 [issues](https://github.com/ctaggart/SourceLink/issues?milestone=1&state=closed)  
 * fixed some relative path issues

2013-07-15 SourceLink.Build 0.2  
 * SourceLink MSBuild task works with git

2013-06-28 SourceLink.Build 0.1  
 * SourceCheck MSBuild task works with git

## Issues
Bugs can be logged in the [issue tracker](https://github.com/ctaggart/SourceLink/issues). Questions can be asked on stackoverflow with a ["sourcelink" tag](http://stackoverflow.com/questions/tagged/sourcelink).  

 * CodePlex needs a raw interface for files. [Vote here!](https://codeplex.codeplex.com/workitem/26806)
 * ReSharper source navigation support is not working with **https**. The [bug](http://youtrack.jetbrains.com/issue/RSRP-371569) is marked critical.
 * NuGet 2.5 MSBuild 2.5 packages [don't work with Package Restore](https://nuget.codeplex.com/workitem/3268). If you are doing builds on a build server, have a look at the workaround and report back.