namespace SourceLink

open System
open System.IO
open LibGit2Sharp
open System.Collections.Generic
open System.Runtime.InteropServices

///// Windows API
//module Windows =
//    [<DllImport("kernel32", CharSet=CharSet.Unicode)>]
//    extern int AddDllDirectory(string dir)

//type internal GitLib() =
//    // similar to static initializer in LibGit2Sharp.Core.NativeMethods,
//    // but it uses location of LibGit2Sharp.dll and AddDllDirectory instead of ExecutingAssembly and PATH
//    static do
//        try
//            let dir = typeof<LibGit2SharpException>.Assembly.Location |> Path.GetDirectoryName
//            let arch = if IntPtr.Size = 8 then "amd64" else "x86"
//            let path = Path.combine dir (sprintf @"NativeBinaries\%s" arch)
//            let i = path |> Windows.AddDllDirectory
//            if i = 0 then
//                Ex.failwithf "AddDllDirectory %A failed: %A" path (ComponentModel.Win32Exception(Marshal.GetLastWin32Error()))
//        with
//            | :? EntryPointNotFoundException -> Ex.failwithf "AddDllDirectory not found. Install KB2533623 update. http://support.microsoft.com/kb/2533623"

type GitRepo(dir) =
    let dir = Path.absolute dir
//    let gl = new GitLib() // trigger static initializer
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