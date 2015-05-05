module SourceLink.SrcToolx

open System.IO
open SourceLink.SymbolStore

let getSourceFilePathAndUrl (pdb: string) =
    use p = File.OpenRead pdb
    let cache = SymbolCache "."
    let pr = cache.ReadPdb p pdb
    pr.Documents
    |> Seq.map (fun d -> d.SourceFilePath, pr.GetDownloadUrl d.SourceFilePath)
    |> List.ofSeq

let run (pdb: string) =
    let i = ref 0
    let pdb = if Path.GetExtension pdb = ".dll" then Path.ChangeExtension(pdb, ".pdb") else pdb
    getSourceFilePathAndUrl pdb
    |> Seq.filter (fun (_, url) -> url.IsSome)
    |> Seq.iter (fun (sf, url) ->
        incr i
        printfn "%s" url.Value )
    printfn "%s: %d source files were extracted." pdb !i