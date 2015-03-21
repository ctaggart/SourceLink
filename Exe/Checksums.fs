module SourceLink.Checksums

open System
open System.Text
open System.Collections.Generic

let run (pdb: string) showFiles showUrls check =

    let urls =
        SrcToolx.getSourceFilePathAndUrl pdb
        |> Seq.filter (fun (sf, url) -> url.IsSome)
        |> Seq.map (fun (sf, url) -> sf, url.Value)
        |> Dictionary.ofTuplesCmp StringComparer.OrdinalIgnoreCase

    let p = new PdbFile(pdb)
    let i = ref 0
    for file, checksum in p.Files do
        incr i
        let sb = StringBuilder()
        sb.Appendf "%s" (Hex.encode checksum)
        if showFiles then
            sb.Appendf " %s" file
        if urls.ContainsKey file then
            if showUrls then
                sb.Appendf " %s " urls.[file]
            if check then
                () // TODO
        printfn "%s" (sb.ToString())

    printfn "%s has %d source files" pdb !i