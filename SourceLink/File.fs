namespace SourceLink

open System
open System.IO
open System.Collections.Generic

module File =
    let srcsrv = "srcsrv"

    let computeChecksums files =
        use md5 = Security.Cryptography.MD5.Create()
        let computeHash file =
            use fs = File.OpenRead file
            md5.ComputeHash fs
        let checksums = Dictionary<string, string>()
        for f in files do
            checksums.[f |> computeHash |> Hex.encode] <- f
        checksums

    let readLines (bytes:byte[]) =
        use sr = new StreamReader(new MemoryStream(bytes))
        seq {
            while not sr.EndOfStream do
                yield sr.ReadLine()
        }
        |> Seq.toArray

    let getParentDirectories file =
        seq {
            let path = ref (Path.GetDirectoryName file)
            while false = String.IsNullOrEmpty !path do
                yield !path
                path := Path.GetDirectoryName !path
        }