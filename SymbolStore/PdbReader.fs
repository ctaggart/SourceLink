namespace SourceLink.SymbolStore

open System
open System.IO
open SourceLink.SymbolStore.CorSym          

type PdbReader(reader:ISymUnmanagedReader, sessionCookie:IntPtr, fileName:string) =

    let moduleCookie =
        if String.IsNullOrEmpty fileName then 0L
        else SrcSrv.LoadModule(sessionCookie, fileName, reader.AsSourceServer)
    let isSourceIndexed = moduleCookie <> 0L
    
    new(pdb, sessionCookie, fileName) =
        PdbReader(ISymUnmanagedReader.Create pdb, sessionCookie, fileName)

    new(pdb:Stream) =
        PdbReader(pdb, IntPtr.Zero, null)
    
    member x.IsSourceIndexed = isSourceIndexed

    member x.GetDownloadUrl sourceFilePath =
        if isSourceIndexed then
            SrcSrv.GetFileUrl(sessionCookie, moduleCookie, sourceFilePath) |> Option.ofNull
        else None

    member x.Reader = reader
    member x.Documents = reader.Documents
    member x.GetMethod token = reader.GetMethod token