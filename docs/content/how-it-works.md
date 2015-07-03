# How SourceLink Works

A `dll` and `pdb` are created by a compiler. SourceLink is used to create a text file ([specification](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx)) that links source files to their raw download URLs. The [pdbstr.exe](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558874.aspx) is used to save the file to a stream in the `pdb`. The NuGet package for `SourceLink.exe` includes `pdbstr.exe`. Windows debuggers use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to download the source files on demand. An `md5` checksum stored in the `pdb` by the compiler must match the `md5` checksum of the file that is downloaded.

For more details, see my first three [SourceLink posts](http://blog.ctaggart.com/search/label/SourceLink) on my blog:

  * 2013-03-31 [Assembly to PDB to Source Files](http://blog.ctaggart.com/2013/03/assembly-to-pdb-to-source-files.html)
  * 2013-06-17 [Making the .NET Open Source More Open](http://blog.ctaggart.com/2013/06/making-net-open-source-more-open.html)
  * 2013-07-15 [Source Linking](http://blog.ctaggart.com/2013/07/source-linking.html)
