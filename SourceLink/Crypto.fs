module SourceLink.Crypto

open System.IO
open System.Security.Cryptography

let hash (ha:HashAlgorithm) file =
    use fs = File.OpenRead file
    ha.ComputeHash fs, file

let hashMD5 file =
    use ha = MD5.Create()
    hash ha file |> fst

let hashesMD5 files =
    use ha = MD5.Create()
    files |> Seq.map (hash ha) |> Array.ofSeq

let hashSHA1 file =
    use ha = SHA1.Create()
    hash ha file |> fst

let hashesSHA1 files =
    use ha = SHA1.Create()
    files |> Seq.map (hash ha) |> Array.ofSeq

let hashSHA256 file =
    use ha = SHA256.Create()
    hash ha file |> fst

let hashesSHA256 files =
    use ha = SHA256.Create()
    files |> Seq.map (hash ha) |> Array.ofSeq