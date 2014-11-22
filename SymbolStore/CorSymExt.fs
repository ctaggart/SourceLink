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

    member x.MethodTokens
        with get() =
            seq {
                for d in x.Documents do
                    for m in x.GetMethodsInDocument d do
                        yield m.GetToken() }
    
    member x.GetMethod token =
        let mutable m = Unchecked.defaultof<ISymUnmanagedMethod>
        match x.GetMethod(token, &m) with
        | S_OK -> Some m
        | _ -> None

    member x.Methods
        with get() =
            x.MethodTokens
            |> Seq.map x.GetMethod
            |> Seq.filter Option.isSome
            |> Seq.map Option.get


type ISymUnmanagedDocument with
    member x.SourceFilePath
        with get() =
            let mutable count = 0
            x.GetURL(0, &count, null)
            let sb = StringBuilder count
            x.GetURL(count, &count, sb)
            sb.ToString()

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


type SequencePoint = {
   Offset: int
   Document: ISymUnmanagedDocument
   Line: int
   Column: int
   EndLine: int
   EndColumn: int
   }


type ISymUnmanagedMethod with
    member x.Token
        with get() =
            let mutable token = 0
            x.GetToken(&token)
            token

    member x.SequencePointCount
        with get() =
            let mutable count = 0
            x.GetSequencePointCount(&count)
            count

    member x.RootScope
        with get() =
            let mutable scope = Unchecked.defaultof<ISymUnmanagedScope>
            x.GetRootScope(&scope)
            scope

    member x.GetScopeFromOffset(offset) =
        let mutable scope = Unchecked.defaultof<ISymUnmanagedScope>
        x.GetScopeFromOffset(offset, &scope)
        scope

//    member x.Namespace
//        with get() =
//            let mutable ns = Unchecked.defaultof<ISymUnmanagedNamespace>
//            x.GetNamespace(&ns) // NIE
//            ns

    member x.SequencePoints
        with get() =
            let mutable count = x.SequencePointCount
            let offsets = Array.zeroCreate count
            let docs = Array.zeroCreate count
            let lines = Array.zeroCreate count
            let columns = Array.zeroCreate count
            let endLines = Array.zeroCreate count
            let endColumns = Array.zeroCreate count
            x.GetSequencePoints(count, &count, offsets, docs, lines, columns, endLines, endColumns)
            let points = Array.zeroCreate count
            for i in 0 .. count - 1 do
                points.[0] <- {
                    Offset = offsets.[i]
                    Document = docs.[i]
                    Line = lines.[i]
                    Column = columns.[i]
                    EndLine = endLines.[i]
                    EndColumn = endColumns.[i]
                    }
            points
            |> Array.filter (fun p -> p :> obj <> null)


type ISymUnmanagedNamespace with
    member x.Name
        with get() =
            let mutable count = 0
            x.GetName(0, &count, null)
            let sb = StringBuilder count
            x.GetName(count, &count, sb)
            sb.ToString()


type ISymUnmanagedScope with
    member x.Method
        with get() =
            let mutable m = Unchecked.defaultof<ISymUnmanagedMethod>
            x.GetMethod(&m)
            m

    member x.Parent
        with get() =
            let mutable parent = Unchecked.defaultof<ISymUnmanagedScope>
            x.GetParent(&parent)
            parent

    member x.Children
        with get() =
            let mutable count = 0
            x.GetChildren(0, &count, null)
            let children = Array.zeroCreate count
            x.GetChildren(count, &count, children)
            children

    member x.StartOffset
        with get() =
            let mutable offset = 0
            x.GetStartOffset(&offset)
            offset

    member x.EndOffset
        with get() =
            let mutable offset = 0
            x.GetEndOffset(&offset)
            offset

    member x.LocalCount
        with get() =
            let mutable count = 0
            x.GetLocalCount(&count)
            count

    member x.Locals
        with get() =
            let mutable count = x.LocalCount
            let locals = Array.zeroCreate count
            x.GetLocals(count, &count, locals)
            locals

    member x.Namespaces
        with get() =
            let mutable count = 0
            x.GetNamespaces(count, &count, null)
            let nss = Array.zeroCreate count
            x.GetNamespaces(count, &count, nss)
            nss