module SourceLink.Build.Proj

open Microsoft.Build.Evaluation
open System.IO

let pathCombine a b = Path.Combine(a, b)

/// gets any file marked as Compile in the project file
let getCompiles file =
    ProjectCollection.GlobalProjectCollection.UnloadAllProjects()
    let p = Project(file:string)
    p.Items
    |> Seq.filter (fun i -> i.ItemType = "Compile")
    |> Seq.map (fun i -> pathCombine (Path.GetDirectoryName file) i.EvaluatedInclude)
    |> Seq.toArray