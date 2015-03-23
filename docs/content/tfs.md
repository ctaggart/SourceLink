# Integrate with TFS

SourceLink has been used to source index Team Foundation Server (TFS) code in an on-premise TFS Git repository as well as [Visual Studio Online](http://www.visualstudio.com/) (VSO). TFS 2013 [does not yet support](http://stackoverflow.com/questions/24663813/is-source-indexing-for-git-builds-possible-in-tfs-2013) source indexing TFS Git code. SourceLink provides a solution for doing so. TFS Build does support source indexing for TFVC, but it requires tf.exe to be run each time a source file is needed. SourceLink uses `https` links for source indexing to obtain the source. This is more secure, faster, and easier to support than having to run an executable.

Either `SourceLink.exe` or `SourceLink.Fake` can be used for source indexing with TFS. The URL to source index with can be derived from the download links in the web UI. In F# sprintf notation it looks like this:

    "%s/_api/_versioncontrol/itemContent?repositoryId=%s&path=/%%var2%%&version=GC{0}&contentOnly=false&__v=5"
    tfsTeamProject repositoryId

## SourceLink.Tfs
`SourceLink.Tfs` is a library bundled with `SourceLink.Fake` that allows you to delegate some or all of a [XAML Build](http://blogs.msdn.com/b/visualstudioalm/archive/2015/02/12/build-futures.aspx) to be in a FAKE Build instead.
`SoureLink.Fake` ships with a TFS build templates [TfsGit.xaml](https://github.com/ctaggart/SourceLink/blob/master/Fake/TfsGit.xaml) and [VsoGit.xaml](https://github.com/ctaggart/SourceLink/blob/master/Fake/VsoGit.xaml). They simply delegate the work to FAKE, allowing you to [code your TFS builds using FAKE instead of XAML](http://blog.ctaggart.com/2014/01/code-your-tfs-builds-in-f-instead-of.html). `SourceLink.Tfs` provides easy access to the entire TFS API from your FAKE build scripts. F# Interactive can be used to connect to TFS and try things out.