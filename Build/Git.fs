module SourceLink.Build.Git

open System
open System.IO
open System.Text
open LibGit2Sharp
open System.Collections.Generic
open SourceLink
open SourceLink.Extension
open SourceLink.Exception

let concatBytes (a:byte[]) (b:byte[]) =
    let c = Array.create (a.Length + b.Length) 0uy
    Buffer.BlockCopy(a, 0, c, 0, a.Length)
    Buffer.BlockCopy(b, 0, c, a.Length, b.Length)
    c

/// compute the git checksums of the source files
let computeChecksums files =
    use sha1 = Security.Cryptography.SHA1.Create()
    files |> Seq.map (fun file ->
        let bytes = File.ReadAllBytes file
        let prefix = sprintf "blob %d%c" bytes.Length (char 0)
        let checksum = sha1.ComputeHash(concatBytes prefix.ToUtf8 bytes) |> Hex.encode
        checksum, file
    )
    |> Seq.toArray

let isRepo dir =
    try
        use repo = new Repository(dir)
        true
    with
    | :? RepositoryNotFoundException -> false

let findRepo file =
    File.getParentDirectories file |> Seq.tryFind isRepo

let getRevision dir =
    use repo = new Repository(dir)
    repo.Head.Tip.Sha

/// get a set of checksums from the git repository for all files that exist
let getChecksums dir files =
    use repo = new Repository(dir)
    let checksums = HashSet(StringComparer.OrdinalIgnoreCase)
    files |> Seq.iter (fun (file:string) ->
        let f =
            if Path.IsPathRooted file then
                file.Substring(dir.Length + 1)
            else
                file
        let ie = repo.Index.[f]
        if ie <> null then
            checksums.Add ie.Id.Sha |> ignore
    )
    checksums