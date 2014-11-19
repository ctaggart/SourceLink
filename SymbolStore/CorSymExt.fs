[<AutoOpen>]
module SourceLink.SymbolStore.CorSymExt

open System
open System.IO
open System.Text
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

type ISymUnmanagedDocument with
    member x.SourceFilePath
        with get() =
            let mutable count = 0
            x.GetURL(0, &count, null)
            let path = StringBuilder count
            x.GetURL(count, &count, path)
            path.ToString()

    member x.DocumentType
        with get() =
            let mutable guid = Guid()
            x.GetDocumentType(&guid)
            guid

    member x.Checksum
        with get() =
            let mutable count = 0
            x.GetCheckSum(0, &count, null) |> ignore
            let data = Array.zeroCreate count
            x.GetCheckSum(count, &count, data) |> ignore
            data

    member x.ChecksumHex with get() = x.Checksum |> Hex.encode