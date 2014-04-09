namespace SourceLink

open System
open System.IO
open LibGit2Sharp
open System.Collections.Generic
open System.Runtime.InteropServices

type GitRepo(dir) =
    let dir = Path.absolute dir
    let repo = new Repository(dir)
    
    member x.Repo with get() = repo
    member x.Revision with get() = repo.Head.Tip.Sha

    static member ComputeChecksums files =
        use sha1 = Security.Cryptography.SHA1.Create()
        files |> Seq.map (fun file ->
            let bytes = File.ReadAllBytes file
            let prefix = sprintf "blob %d%c" bytes.Length (char 0)
            let checksum = sha1.ComputeHash(Byte.concat prefix.ToUtf8 bytes) |> Hex.encode
            checksum, file
        )
        |> Seq.toArray

    static member ComputeChecksum file =
        GitRepo.ComputeChecksums [file] |> Seq.head |> fst

    member x.Checksums files =
        files |> Seq.map (fun (file:string) ->
            let f =
                if Path.IsPathRooted file then
                    file.Substring(dir.Length + 1)
                else
                    file
            let ie = repo.Index.[f]
            if ie <> null then file, ie.Id.Sha
            else file, ""
        )

    member x.Checksum file =
        x.Checksums [file] |> Seq.head |> snd

    member x.ChecksumSet files = 
        let checksums = 
            x.Checksums files
            |> Seq.map snd
            |> Seq.filter (not << String.IsNullOrEmpty)
        HashSet(checksums, StringComparer.OrdinalIgnoreCase)

    /// returns a sorted list of files with checksums that do not match
    member x.VerifyFiles files =
        let committed = x.ChecksumSet files
        let different = SortedSet(StringComparer.OrdinalIgnoreCase)
        for checksum, file in GitRepo.ComputeChecksums files do
            if false = committed.Contains checksum then
                different.Add file |> ignore
        different |> Array.ofSeq

    static member IsRepo dir =
        try
            use repo = new Repository(dir)
            true
        with
        | :? RepositoryNotFoundException -> false

    static member Find file = Path.GetDirectoryNames file |> Seq.tryFind GitRepo.IsRepo

    member x.Paths (files:seq<string>) = files |> Seq.map (fun f -> f, f.Substring(dir.Length+1).Replace('\\','/'))

    member x.Dispose() =
        use repo = repo
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()