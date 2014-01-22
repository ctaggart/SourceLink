// http://en.wikipedia.org/wiki/Directed_acyclic_graph
module SourceLink.Digraph

open System
open System.Collections.Generic

// http://en.wikipedia.org/wiki/Topological_sorting
/// topological sort, depth-first search
/// pass in the nodes, a comparer for identity, and a function to get the referenced nodes
let topSort  (nodes: seq<'T>) (cmp:IEqualityComparer<'T>) (refs: 'T -> seq<'T>) : seq<'T> =
    seq {
        let visited = HashSet cmp
        let rec visit n =
            seq {
                if visited.Add n then
                    for m in refs n do
                        yield! visit m
                    yield n
            }
        for n in nodes do
            yield! visit n
    }