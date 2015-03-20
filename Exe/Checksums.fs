module SourceLink.Checksums

let run (pdb: string) =
    let p = new PdbFile(pdb)

    let i = ref 0
    for file, checksum in p.Files do
        incr i
        printfn "%s %s" (Hex.encode checksum) file

    printfn "%s has %d source files" pdb !i