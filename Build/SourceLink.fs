namespace SourceLink.Build

open System
open System.IO
open Microsoft.Build.Framework
open LibGit2Sharp
open System.Collections.Generic
open SourceLink

type SourceLink() =
    inherit SourceCheck()

    [<Required>]
    member val TargetPath = String.Empty with set, get

    [<Required>]
    member val RepoUrl = String.Empty with set, get

    member val WriteSrcSrvTxt = false with set, get

    override x.Execute() =
        let repoDir = x.GetRepoDir()
        let pdbFile = Path.ChangeExtension(x.TargetPath, ".pdb")
        
        if false = File.Exists pdbFile then
            x.Error "can not find pdb: %s" pdbFile
        
        if false = x.HasErrors then
            try
                let revision = Git.getRevision repoDir
                x.MessageHigh "pdb source linking revision %s in %s" revision x.RepoUrl

                let files = x.GetSourceFiles()
                let fileChecksums = File.computeChecksums files

                let sourceFiles = SortedDictionary(StringComparer.OrdinalIgnoreCase) // file, path
                let missingFiles = SortedDictionary(StringComparer.OrdinalIgnoreCase) // file, checksum

                use pdb = new PdbFile(pdbFile)
                let pdbChecksums = PdbChecksums(pdb)
                for checksum in pdbChecksums.ChecksumToFilename.Keys do
                    let file = pdbChecksums.ChecksumToFilename.[checksum]
                    if fileChecksums.ContainsKey checksum then
                        let path = file.Substring(repoDir.Length+1).Replace('\\','/')
                        sourceFiles.Add(file,path)
                    else
                        missingFiles.Add(file,checksum)

                if missingFiles.Count > 0 then
                    x.Error "cannot find %d source files" missingFiles.Count
                    for KeyValue(file, checksum) in missingFiles do
                        x.Error "cannot find %s with checksum of %s" file checksum

                if false = x.HasErrors then
                    let srcFiles = sourceFiles |> Seq.map (fun (KeyValue(file,path)) -> file, path) |> Seq.toArray
                    let srcsrv = SrcSrv.createSrcSrv x.RepoUrl revision srcFiles
                    if x.WriteSrcSrvTxt then
                        File.WriteAllBytes(pdbFile + ".srcsrv.txt", srcsrv)
                    pdb.FreeInfoPages()
                    pdb.WriteSrcSrv srcsrv
                    pdb.Info.Age <- pdb.Info.Age + 1
                    pdb.Save()
                
            with
            | :? RepositoryNotFoundException as ex -> x.Error "%s" ex.Message
            | :? SourceLinkException as ex -> x.Error "%s" ex.Message
        not x.HasErrors
    