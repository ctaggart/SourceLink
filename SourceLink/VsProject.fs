[<AutoOpen>]
module SourceLink.VsBuild

open Microsoft.Build.Evaluation
open System.IO
open SourceLink
    
type VsProject = Project // abbreviation

type Project with
    /// full path for all "Compile" items
    member x.ItemsCompile
        with get() =
            x.Items
            |> Seq.filter (fun i -> i.ItemType = "Compile")
            |> Seq.map (fun i -> i.EvaluatedInclude)
            |> Seq.map (fun path -> Path.combine (Path.GetDirectoryName x.FullPath) path |> Path.GetFullPath)
            |> Seq.toList