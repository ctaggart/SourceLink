[<AutoOpen>]
module SourceLink.PdbChecksums

open System
open System.Collections.Generic

type PdbFile with
    
    member x.Files
        with get() = 
            let prefix = "/src/files/"
            x.Info.NameToPdbName.Values
            |> Seq.filter (fun pdbName -> pdbName.Name.StartsWith prefix)
            |> Seq.map (fun pdbName -> pdbName.Name.Substring prefix.Length, x.ReadStreamBytes pdbName.Stream)
            |> Seq.filter (fun (file, bytes) -> bytes.Length = 0x58)
            |> Seq.map (fun (file, bytes) -> file, bytes.[0x48..0x57])

    member x.Checksums
        with get() =
            let d = Dictionary StringComparer.OrdinalIgnoreCase
            x.Files
            |> Seq.map (fun (file, hash) -> Hex.encode hash, file)
            |> d.AddAll
            d

    member x.VerifyChecksums files =
        let missing = SortedDictionary StringComparer.OrdinalIgnoreCase // file, checksum
        let pdbChecksums = x.Checksums
        let fileChecksums =
            let d = Dictionary StringComparer.OrdinalIgnoreCase
            Crypto.hashesMD5 files 
            |> Seq.map (fun (hash, file) -> Hex.encode hash, file)
            |> d.AddAll
            d
        for checksum, file in pdbChecksums.KeyValues do
            if fileChecksums.ContainsKey checksum = false then
                missing.[file] <- checksum
        missing