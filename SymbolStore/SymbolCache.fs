namespace SourceLink.SymbolStore

open System
open System.IO
open SourceLink.SymbolStore.CorSym

type SymbolCache (symbolCacheDir:string) =
    let sessionCookie = IntPtr(Random().Next())
    do SrcSrv.Init(sessionCookie, symbolCacheDir)

    member x.ReadPdb (stream:Stream) filePath =
        PdbReader(stream, sessionCookie, filePath)

    member x.DownloadFile downloadUrl =
        SymSrv.DownloadFile(downloadUrl, symbolCacheDir)