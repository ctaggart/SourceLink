[<AutoOpen>]
module SourceLink.VsBuild

open System
open System.IO
open Microsoft.Build.Evaluation
open System.Collections.Generic
open SourceLink

type VsProject = Project // abbreviation

type Project with
    static member Load (projectFile:string) globalProps = Project(projectFile, globalProps |> Dictionary.ofTuples, null)

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
            Path.combine dir (sprintf "%s%s" x.AssemblyName ext)

    member x.OutputFilePdb with get() = Path.ChangeExtension(x.OutputFile, ".pdb")