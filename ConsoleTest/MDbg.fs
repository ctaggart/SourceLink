namespace SourceLink

open Microsoft.Samples.Debugging.SymbolStore
open Microsoft.Samples.Debugging.CorSymbolStore

type SequencePoint = {
   Offset: int
   Document: ISymbolDocument
   Line: int
   Column: int
   EndLine: int
   EndColumn: int
   }

[<AutoOpen>]
module MDbg =

    type ISymbolReader with
        static member Create symUnmanagedReader =
            SymbolBinder.GetReaderFromCOM symUnmanagedReader

    type ISymbolMethod with
        member x.SequencePoints
            with get() =
                let count = x.SequencePointCount
                let offsets = Array.zeroCreate count
                let docs = Array.zeroCreate count
                let lines = Array.zeroCreate count
                let columns = Array.zeroCreate count
                let endLines = Array.zeroCreate count
                let endColumns = Array.zeroCreate count
                x.GetSequencePoints(offsets, docs, lines, columns, endLines, endColumns)
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
