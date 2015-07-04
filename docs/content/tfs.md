# Source Index to TFS

SourceLink.exe may be used to source index pdb files to on-premise TFS Git repositories. The `--url` maps to the download url for the repository, for example:

    "$tfs/_api/_versioncontrol/itemContent?repositoryId=$repo&path=%var2&version=GC{0}&contentOnly=false&__v=5"

TFS 2013 did not support source indexing Git repositories, but it [appears that TFS 2015 will](http://stackoverflow.com/a/30904906/23059). Unfortunately, they still do source indexing with `tf.exe`. Using `https` instead is more secure, faster, and easier to support than having to run an executable. I'll be reaching out to their team to see if SourceLink can be used.

### Can it be used with TFVC?
It should work with Team Foundation Version Control too instead of Git. Just disable the Git repository verification with `--not-verify-git`. `SourceLink.exe checksums --check` can be used after indexing to test download all the index source files and check their checksums.

![](https://cloud.githubusercontent.com/assets/80104/8490561/f873cee2-20df-11e5-95ee-b64d96418c93.png)

### With FAKE

A large amount of work was done to integrate FAKE with TFS 2013 and [Visual Studio Online](http://www.visualstudio.com/). SourceLink.Tfs is available on NuGet and it also ships with SourceLink.Fake. Here are some [blog posts about it](http://blog.ctaggart.com/search/label/TFS).

It is a way code FAKE Build scripts for TFS in F# instead of [XAML Build](http://blogs.msdn.com/b/visualstudioalm/archive/2015/02/12/build-futures.aspx) in xml. F# Interactive can be used to connect to TFS and try things out. You still have access to the entire TFS .NET API. Many F# helpers are provided in SourceLink.Tfs.

SoureLink.Fake ships with a TFS build templates [TfsGit.xaml](https://github.com/ctaggart/SourceLink/blob/master/Fake/TfsGit.xaml) and [VsoGit.xaml](https://github.com/ctaggart/SourceLink/blob/master/Fake/VsoGit.xaml). They simply delegate the work to FAKE, allowing you to [code your TFS builds using FAKE instead of XAML](http://blog.ctaggart.com/2014/01/code-your-tfs-builds-in-f-instead-of.html).
