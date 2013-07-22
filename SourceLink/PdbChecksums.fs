namespace SourceLink

open System
open System.Collections.Generic
open SourceLink.Exception

type PdbChecksums(file:PdbFile) =
    
    let filenameToChecksum = SortedDictionary(StringComparer.OrdinalIgnoreCase)
    let checksumToFilename = Dictionary(StringComparer.OrdinalIgnoreCase)
    do // read checksums
        let prefix = "/src/files/"
        file.Info.NameToPdbName.Values
        |> Seq.filter (fun name -> name.Name.StartsWith prefix)
        |> Seq.iter (fun name ->
            let bytes = file.ReadStreamBytes name.Stream
            let filename = name.Name.Substring prefix.Length
            if bytes.Length = 0x58 then
                let checksum = bytes.[0x48..0x57] |> Hex.encode
                filenameToChecksum.[filename] <- checksum
                checksumToFilename.[checksum] <- filename
//            else
//                failwithf "unable to read checksum for %s" filename
        )

    member x.FilenameToChecksum with get() = filenameToChecksum :> IDictionary<string,string>
    member x.ChecksumToFilename with get() = checksumToFilename :> IDictionary<string,string>

