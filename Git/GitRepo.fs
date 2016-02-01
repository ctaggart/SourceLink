namespace SourceLink

open System
open System.IO
open LibGit2Sharp
open System.Collections.Generic
open System.Runtime.InteropServices

type GitChecksum = {
    File: string
    ChecksumOfFile: string
    ChecksumInGit: string }

type GitChecksums = {
    Matched: List<GitChecksum>
    Unmatched: List<GitChecksum> }

type GitRepo(dir) =
    let dir = Path.absolute dir
    let repo = new Repository(dir)
    
    member x.Repo with get() = repo
    member x.Commit with get() = repo.Head.Tip.Sha
    [<Obsolete("use .Commit instead")>]
    member x.Revision with get() = x.Commit

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

    member x.Checksum file =
        let f =
            if Path.IsPathRooted file then
                file.Substring(dir.Length + 1)
            else
                file
        match repo.Index.[f] with
        | null -> ""
        | ie -> ie.Id.Sha

    member x.Checksums files =
        files |> Seq.map (fun file -> file, x.Checksum file)

    member x.ChecksumSet files = 
        let checksums = 
            x.Checksums files
            |> Seq.map snd
            |> Seq.filter (not << String.IsNullOrEmpty)
        HashSet(checksums, StringComparer.OrdinalIgnoreCase)

    member x.MatchChecksums files =
        let matched = List<_>()
        let unmatched = List<_>()
        for checksum, file in GitRepo.ComputeChecksums files do
            let gitChecksum = x.Checksum file
            let gc = { File = file; ChecksumOfFile = checksum; ChecksumInGit = gitChecksum }
            if checksum = gitChecksum then
                matched.Add gc
            else unmatched.Add gc
        { Matched = matched; Unmatched = unmatched }

    /// returns a sorted list of files with checksums that do not match
//    [<Obsolete "use .MatchChecksums instead">]
    member x.VerifyFiles files =
        let mc = x.MatchChecksums files
        let different = SortedSet(StringComparer.OrdinalIgnoreCase)
        for gc in mc.Unmatched do
            different.Add gc.File |> ignore
        different |> Array.ofSeq

    static member IsRepo dir =
        try
            use repo = new Repository(dir)
            true
        with
        | :? RepositoryNotFoundException -> false

    static member TryFind file =
        Path.GetDirectoryNames file |> Seq.tryFind GitRepo.IsRepo

    static member Find file =
        match GitRepo.TryFind file with
        | Some repo -> repo
        | None -> sprintf "git repository not found for %s" file |> RepositoryNotFoundException |> raise

    member x.Paths (files:seq<string>) =
        files |> Seq.map (fun f -> f, f.Substring(dir.Length+1).Replace('\\','/'))

    member x.Dispose() =
        use repo = repo
        GC.SuppressFinalize x
    interface IDisposable with member x.Dispose() = x.Dispose() 
    override x.Finalize() = x.Dispose()