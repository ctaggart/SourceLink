namespace SourceLink.SymbolStore

open System
open System.IO
open SourceLink.SymbolStore.CorSym          

type PdbReader(reader:ISymUnmanagedReader, sessionCookie:IntPtr, fileName:string) =
    
    let moduleCookie =
        if String.IsNullOrEmpty fileName then 0L
        else SrcSrv.LoadModule(sessionCookie, fileName, reader.AsSourceServer)
    let isSourceIndexed = moduleCookie <> 0L
    
    static let createReader pdb fileName = 
        try
            ISymUnmanagedReader.Create pdb
        with
            | _ -> failwithf "error reading pdb file '%s'" fileName

    new(pdb, sessionCookie, fileName) =
        PdbReader(createReader pdb fileName, sessionCookie, fileName)

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