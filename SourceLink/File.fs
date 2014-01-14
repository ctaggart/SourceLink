module SourceLink.File

open System
open System.IO
open System.Collections.Generic

let srcsrv = "srcsrv"

let readLines (bytes:byte[]) =
    use sr = new StreamReader(new MemoryStream(bytes))
    seq {
        while not sr.EndOfStream do
            yield sr.ReadLine()
    }
    |> Seq.toArray

// File.GetParents
let getParentDirectories file =
    seq {
        let path = ref (Path.GetDirectoryName file)
        while false = String.IsNullOrEmpty !path do
            yield !path
            path := Path.GetDirectoryName !path
    }