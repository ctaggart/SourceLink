namespace SourceLink

open System

type SourceLinkException(msg) =
    inherit Exception(msg)

module Ex =
    let failwithf fmt  = Printf.ksprintf (fun s -> SourceLinkException s |> raise) fmt