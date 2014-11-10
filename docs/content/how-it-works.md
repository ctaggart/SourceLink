# How SourceLink Works

A `dll` and `pdb` are created by a compiler. SourceLink is used to create a text file ([specification](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx)) that links source files to their raw download URLs. The [pdbstr.exe](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558874.aspx) is used to save the file to a stream in the `pdb`. `pdbstr.exe` ships with TFS and [Debugging Tools for Windows](http://msdn.microsoft.com/en-us/windows/hardware/hh852365.aspx). Windows debuggers use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to download the source files on demand. An `md5` checksum stored in the `pdb` by the compiler must match the `md5` checksum of the file that is downloaded.

For more details, see my first three [SourceLink posts](http://blog.ctaggart.com/search/label/SourceLink) on my blog:

  * 2013-03-31 [Assembly to PDB to Source Files](http://blog.ctaggart.com/2013/03/assembly-to-pdb-to-source-files.html)
  * 2013-06-17 [Making the .NET Open Source More Open](http://blog.ctaggart.com/2013/06/making-net-open-source-more-open.html)
  * 2013-07-15 [Source Linking](http://blog.ctaggart.com/2013/07/source-linking.html)

## Line Endings
The checksums of the files that the compiler uses are stored in the `pdb`. Unfortunately, Git on Windows behaves like `core.autocrlf=true` by default. It will auto convert the line endings to `crlf` (carriage return, line feed) instead of leaving them just `lf` (line feed). They are stored in the Git repository as `lf` if you used a `.gitattributes` file which is recommended. Here are a some different ways to keep Git on Windows from auto converting line endings.

  * configure git to use core.autocrlf input globally *before* cloning
    
    `git config --global core.autocrlf input`

    often done with AppVeyor in `init` section
    
    examples: [octokit.net](https://github.com/octokit/octokit.net/blob/master/appveyor.yml#L2), [FSharp.Data](https://github.com/fsharp/FSharp.Data/blob/master/appveyor.yml#L2)

  * configure git to use core.autocrlf input *when* cloning
    
    `git clone https://github.com/octokit/octokit.net.git -c core.autocrlf=input`

  * configure the source files to default to `lf` in `.gitattributes`

    `*.cs eol=lf`

    `*.fs eol=lf`

    examples: [FSharp.Compiler.Service](https://github.com/fsharp/FSharp.Compiler.Service/blob/master/.gitattributes#L5), [SourceLink](https://github.com/ctaggart/SourceLink/blob/master/.gitattributes#L5)

    This just changes the default behavior. It can still be overriden using `core.autocrlf=true` if someone really wants `crlf`.