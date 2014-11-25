[<AutoOpen>]
module SourceLink.VsBuild

open System
open System.IO
open Microsoft.Build.Evaluation
open System.Collections.Generic
open SourceLink

type VsProj = Project // abbreviation

type Project with
    static member Load (proj:string) globalProps =
        try
            let pc = new ProjectCollection()
            Project(proj, globalProps |> Dictionary<_,_>.ofTuples, null, pc)
        with
        | ex -> failwithf "unable to load proj file `%s` with properties: %A, error %s" proj globalProps ex.Message
    static member LoadRelease proj = Project.Load proj ["Configuration","Release"]

    /// full path for all "Compile" items
    member x.ItemsCompile
        with get() =
            x.Items
            |> Seq.filter (fun i -> i.ItemType = "Compile")
            |> Seq.map (fun i -> i.EvaluatedInclude)
            |> Seq.map (fun path -> Path.combine (Path.GetDirectoryName x.FullPath) path |> Path.GetFullPath)
            |> Seq.toList

    member x.OutputPath with get() = (x.GetProperty "OutputPath").EvaluatedValue
    member x.AssemblyName with get() = (x.GetProperty "AssemblyName").EvaluatedValue
    member x.OutputType with get() = (x.GetProperty "OutputType").EvaluatedValue

    member x.OutputFile
        with get() =
            let ext =
                let ot = x.OutputType
                if "Library".EqualsI ot then ".dll"
                elif "Exe".EqualsI ot || "WinExe".EqualsI ot then ".exe"
                else Ex.failwithf "OutputType not matched: %s" ot
            let dir = Path.combine x.DirectoryPath x.OutputPath
            Path.combine dir (sprintf "%s%s" x.AssemblyName ext) |> Path.GetFullPath
    member x.OutputDirectory with get() = Path.GetDirectoryName x.OutputFile

    member x.OutputFilePdb with get() = Path.ChangeExtension(x.OutputFile, ".pdb")
    member x.OutputFilePdbSrcSrv with get() = x.OutputFilePdb + ".srcsrv"

    member x.VerifyPdbFiles (files:seq<string>) = 
        use pdb = new PdbFile(x.OutputFilePdb)
        pdb.VerifyChecksums files

    member x.CreateSrcSrv rawUrl revision paths =
        File.WriteAllBytes(x.OutputFilePdbSrcSrv, SrcSrv.create rawUrl revision paths)