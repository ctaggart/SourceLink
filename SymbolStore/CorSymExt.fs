[<AutoOpen>]
module SourceLink.SymbolStore.CorSymExt

open System.IO
open SourceLink.SymbolStore.CorSym

type ISymUnmanagedReader with
    static member Create pdb =
        TempPdbReader.CreateRawReader pdb

    member x.AsReader2 = x :?> ISymUnmanagedReader2
    member x.AsSourceServer = x :?> ISymUnmanagedSourceServerModule

    member x.Documents
        with get() =
            let mutable count = 0
            x.GetDocuments(0, &count, null)
            let docs = Array.zeroCreate count
            x.GetDocuments(count, &count, docs)
            docs

    member x.GetMethodsInDocument doc =
        let mutable count = 0
        x.AsReader2.GetMethodsInDocument(doc, 0, &count, null)
        let methods = Array.zeroCreate count
        x.AsReader2.GetMethodsInDocument(doc, count, &count, methods)
        methods