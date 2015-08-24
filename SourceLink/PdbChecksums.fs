[<AutoOpen>]
module SourceLink.PdbChecksums

open System
open System.Collections.Generic

type PdbChecksum = {
    File: string
    ChecksumOfFileBytes: byte[]
    ChecksumInPdbBytes: byte[] }
    
    with
        member x.ChecksumOfFile = Hex.encode x.ChecksumOfFileBytes
        member x.ChecksumInPdb = Hex.encode x.ChecksumInPdbBytes

type PdbChecksums = {
    Matched: List<PdbChecksum>
    Unmatched: List<PdbChecksum> }
   
    with 
        member x.MatchedFiles =
            x.Matched |> Seq.map (fun fc -> fc.File) |> List.ofSeq

type PdbFile with
    
    /// A sequence of files and their checksums
    member x.Files
        with get() = 
            let prefix = "/src/files/"
            x.Info.NameToPdbName.Values
            |> Seq.filter (fun pdbName -> pdbName.Name.StartsWith prefix)
            |> Seq.map (fun pdbName -> pdbName.Name.Substring prefix.Length, x.ReadStreamBytes pdbName.Stream)
            |> Seq.filter (fun (file, bytes) ->
                match bytes.Length with
                | 88 -> true // MD5 is last 16 bytes
                | 92 -> true // SHA-1 is last 20 bytes
                | 104 -> true // SHA-256 is last 32 bytes
                | _ -> false)
            |> Seq.map (fun (file, bytes) -> file, bytes.[72..])

    /// A set of files and their checksums
    member x.FileSet
        with get() =
            let d = Dictionary StringComparer.OrdinalIgnoreCase
            x.Files
            |> Seq.map (fun (file, checksum) -> file, checksum)
            |> d.AddAll
            d
    
    /// Computes the checksums for the list of files passed in and verifies that the pdb contains them.
    /// Returns a list of matched and unmatched files and their checksums.
    /// Only matches when filenames match.
    member x.MatchChecksums files =
        let matched = List<_>()
        let unmatched = List<_>()
        let pdbFiles = x.FileSet
        for file in files do
            if pdbFiles.ContainsKey file then
                let checksumInPdb = pdbFiles.[file]
                let checksum =
                    match checksumInPdb.Length with
                    | 16 -> Crypto.hashMD5 file
                    | 20 -> Crypto.hashSHA1 file
                    | 32 -> Crypto.hashSHA256 file
                    | _ -> Array.empty
                let pc = { File = file; ChecksumOfFileBytes = checksum; ChecksumInPdbBytes = checksumInPdb }
                if checksum.CollectionEquals checksumInPdb then
                    matched.Add pc
                else unmatched.Add pc
        { Matched = matched; Unmatched = unmatched }

    [<Obsolete "use .MatchChecksums instead">]
    member x.VerifyChecksums files =
        let missing = SortedDictionary StringComparer.OrdinalIgnoreCase // file, checksum
        for um in (x.MatchChecksums files).Unmatched do
            missing.[um.File] <- um.ChecksumOfFile
        missing