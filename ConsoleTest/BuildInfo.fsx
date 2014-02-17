#load "../packages/SourceLink.Fake/tools/Assemblies.fsx"
open System
open SourceLink
let t = new Tfs(Uri "https://ctaggart.visualstudio.com/DefaultCollection")

type Object with
    member x.TypeString = x.GetType().ToString()

open Microsoft.TeamFoundation.Build.Client
let rec printNode indent (node:IBuildInformationNode) =
    printfn "%s%s" indent node.Type
    for f in node.Fields do
        printfn "%s    %s = %s" indent f.Key f.Value
    for n in node.Children.Nodes do
        printNode (indent+"  ") n

let b = t.BuildServer.GetBuild 308
let i = b.Information

i.Nodes.Length
let activityTracking = i.Nodes.[0]
printNode "" activityTracking

let configurationSummary = i.Nodes.[1]
printNode "" configurationSummary

activityTracking.Children.Nodes.Length
let atn = activityTracking.Children.Nodes.[0]
