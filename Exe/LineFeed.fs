module SourceLink.LineFeed

open System.IO
open SourceLink
open System.Text

let hasCrLf (lines: string seq) =
    lines |> Seq.exists (fun line -> line.EndsWith "\r")

let run (proj:string option) (projProps:(string * string) list)
    (files:string list) (notFiles:string list) =
    
    verbosefn "proj: %A" proj
    verbosefn "projProps: %A" projProps
    verbosefn "files: %A" files
    verbosefn "notFiles: %A" notFiles

    let pFiles, pPdbs =
        match proj with
        | None -> [], []
        | Some proj ->
            let p = VsProj.Load proj projProps
            p.ItemsCompilePath, [p.OutputFilePdb]

    let cd = Directory.GetCurrentDirectory()
    let projectFiles = {BaseDirectory=cd; Includes= pFiles @ files; Excludes=notFiles} |> List.ofSeq

    verbosefn "globbed projectFiles: %A" projectFiles

    for pf in projectFiles do
        let lines, encoding =
            // passing UTFEncoding without the BOM set allows it to be detected
            // http://stackoverflow.com/a/27976558/23059
            use sr = new StreamReader(pf, UTF8Encoding false);
            let text = sr.ReadToEnd()
            let lines = text .Split [|'\n'|]
            lines, sr.CurrentEncoding
        
        if lines.Length > 0 && hasCrLf lines then
            tracefn "updating %s" pf

            match encoding with
            | :? UTF8Encoding as utf8 -> verbosefn "  %A, bom: %A" encoding (utf8.GetPreamble())
            | _ -> verbosefn "  %A" encoding

            use f = new StreamWriter(File.Open(pf, FileMode.Create), encoding); // overwrite
            for i in 0 .. lines.Length - 2 do
                f.Write (lines.[i].TrimEnd [|'\r'|])
                f.Write '\n' // LF
            f.Write (lines.[lines.Length - 1].TrimEnd [|'\r'|])