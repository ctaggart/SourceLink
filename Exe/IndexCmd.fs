module SourceLink.IndexCmd

let run (proj:string option) (projProps:(string * string) list)
    (url:string option) (commit:string option)
    (pdbs:string list)
    (verifyGit:bool) (verifyPdb:bool) =
    
    verbosefn "proj: %A" proj
    verbosefn "projProps: %A" projProps
    verbosefn "url: %A" url
    verbosefn "commit: %A" commit
    verbosefn "pdbs: %A" pdbs
    verbosefn "verifyGit: %A" verifyGit
    verbosefn "verifyPdb: %A" verifyPdb

    // TODO #53 implement it

    ()