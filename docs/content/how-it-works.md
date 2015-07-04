# How SourceLink Works

A `dll` and `pdb` are created by a compiler. SourceLink is used to create a text file ([specification](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx)) that links source files to their raw download URLs. The [pdbstr.exe](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558874.aspx) is used to save the file to a stream in the `pdb`. The NuGet package for `SourceLink.exe` includes `pdbstr.exe`. Windows debuggers use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to download the source files on demand. An `md5` checksum stored in the `pdb` by the compiler must match the `md5` checksum of the file that is downloaded. This is why [line endings](line-endings.html) are important.

## Symbol Server vs Source Server
A symbol server serves .pdb files. A source server serves source files such as .cs and .fs files.

### Symbol Server (TODO)
Kinds: next to dll, local, network share, http

### Source Server (TODO)
What the debugger does, looks up a symbol, determines the file, determines the url to download the file
