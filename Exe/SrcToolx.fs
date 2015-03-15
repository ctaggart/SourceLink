module SourceLink.SrcToolx

open System.IO
open SourceLink.SymbolStore

let run (pdb: string) =
    use p = File.OpenRead pdb
    let cache = SymbolCache "."
    let pr = cache.ReadPdb p pdb
    let i = ref 0
    pr.Documents
    |> Seq.map (fun d -> pr.GetDownloadUrl d.SourceFilePath)
    |> Seq.filter (fun url -> url.IsSome)
    |> Seq.iter (fun url ->
        incr i
        printfn "%s" url.Value
    )
    printfn "%s: %d source files were extracted." pdb !i