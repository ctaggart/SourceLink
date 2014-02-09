module SourceLink.Build.Proj

open Microsoft.Build.Evaluation
open System.Collections.Generic
open SourceLink

/// gets any file marked as Compile in the project file
let getCompiles (file:string) (excludes:ISet<string>) =
    ProjectCollection.GlobalProjectCollection.UnloadAllProjects()
    let p = Project(file)
    p.ItemsCompile
