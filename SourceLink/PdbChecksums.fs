[<AutoOpen>]
module SourceLink.PdbChecksums

open System
open System.Collections.Generic

type FileChecksum = {
    File: string
    Checksum: string }

type PdbChecksums = {
    Matched: List<FileChecksum>
    Unmatched: List<FileChecksum> }
   
    with 
        member x.MatchedFiles =
            x.Matched |> Seq.map (fun fc -> fc.File) |> List.ofSeq

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
    
    /// Computes the checksums for the list of files passed in and verifies that the pdb contains them.
    /// Returns a sorted list of matched and unmatched files and their checksums.
    member x.MatchChecksums files =
        let matched = List<_>()
        let unmatched = List<_>()
        let pdbChecksums = x.Checksums
        let fileChecksums =
            let d = Dictionary StringComparer.OrdinalIgnoreCase
            Crypto.hashesMD5 files 
            |> Seq.map (fun (hash, file) -> Hex.encode hash, file)
            |> d.AddAll
            d
        for checksum, file in fileChecksums.KeyValues do
            if pdbChecksums.ContainsKey checksum then
                matched.Add { File = file; Checksum = checksum }
            else
                unmatched.Add { File = file; Checksum = checksum }
        { Matched = matched; Unmatched = unmatched }

    [<Obsolete "use .MatchChecksums instead">]
    member x.VerifyChecksums files =
        let missing = SortedDictionary StringComparer.OrdinalIgnoreCase // file, checksum
        for um in (x.MatchChecksums files).Unmatched do
            missing.[um.File] <- um.Checksum
        missing