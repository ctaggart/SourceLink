namespace SourceLink

open System
open System.Collections.Generic

type PdbChecksums(file:PdbFile) =
    
    let filenameToChecksum = SortedDictionary(StringComparer.OrdinalIgnoreCase)
    let checksumToFilename = Dictionary(StringComparer.OrdinalIgnoreCase)
    do // read checksums
        let prefix = "/src/files/"
        file.Info.NameToPdbName.Values
        |> Seq.filter (fun name -> name.Name.StartsWith prefix)
        |> Seq.iter (fun name -> 
            let checksum = (file.ReadStreamBytes name.Stream).[72..87] |> Hex.encode
            let filename = name.Name.Substring prefix.Length
            filenameToChecksum.[filename] <- checksum
            checksumToFilename.[checksum] <- filename
        )

    member x.FilenameToChecksum with get() = filenameToChecksum :> IDictionary<string,string>
    member x.ChecksumToFilename with get() = checksumToFilename :> IDictionary<string,string>

