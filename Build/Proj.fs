module SourceLink.Build.Proj

open Microsoft.Build.Evaluation
open System.IO
open System.Collections.Generic

let pathCombine a b = Path.Combine(a, b)

/// gets any file marked as Compile in the project file
let getCompiles (file:string) (excludes:ISet<string>) =
    ProjectCollection.GlobalProjectCollection.UnloadAllProjects()
    let dir = Path.GetDirectoryName file
    let p = Project(file)
    p.Items
    |> Seq.filter (fun i -> i.ItemType = "Compile")
    |> Seq.map (fun i -> i.EvaluatedInclude)
    |> Seq.filter (fun path -> false = excludes.Contains path)
    |> Seq.map (fun path -> pathCombine dir path )
    |> Seq.toArray
