[<AutoOpen>]
module Printfn

let printfn format = Printf.ksprintf (fun message -> System.Diagnostics.Debug.WriteLine message) format