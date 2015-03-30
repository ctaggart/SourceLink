#r "System.Xml"
#r "Microsoft.Build"
#r "Microsoft.Build.Framework"

open Microsoft.Build.Evaluation
open System.Collections.Generic

printfn "%s" typeof<ProjectCollection>.Assembly.Location

let pc = new ProjectCollection()
let d = Dictionary()
d.Add("Configuration", "Release")
let p = Project("vsversion.proj", d, null, pc)

let show = [ "MSBuildToolsVersion"; "MSBuildExtensionsPath"; "VisualStudioVersion" ] |> Set.ofList

p.Properties
|> Seq.filter (fun pr -> show.Contains pr.Name)
|> Seq.iter (fun pr ->
    printfn "%s %s" pr.Name pr.UnevaluatedValue
)