module SourceLink.Pdbstrr

let run (pdb: string) =
    use p = new PdbFile(pdb)
    for line in p.ReadSrcSrvLines() do
        printfn "%s" line