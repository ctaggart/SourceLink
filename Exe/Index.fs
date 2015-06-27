module SourceLink.Index

open System.IO
open SourceLink

let run (proj:string option) (projProps:(string * string) list)
    (url:string) (commit:string option)
    (pdbs:string list)
    (verifyGit:bool) (verifyPdb:bool) 
    (files:string list) (notFiles:string list)
    (repoDir:string) 
    (paths:(string * string) list) =
    
    // skip verify if paths are set
    let noPaths = paths.Length = 0
    let verifyGit = verifyGit && noPaths 
    let verifyPdb = verifyPdb && noPaths

    verbosefn "proj: %A" proj
    verbosefn "projProps: %A" projProps
    verbosefn "url: %A" url
    verbosefn "commit: %A" commit
    verbosefn "pdbs: %A" pdbs
    verbosefn "verifyGit: %A" verifyGit
    verbosefn "verifyPdb: %A" verifyPdb
    verbosefn "files: %A" files
    verbosefn "notFiles: %A" notFiles
    verbosefn "repoDir: %A" repoDir
    verbosefn "paths: %A" paths

    let pFiles, pPdbs =
        match proj with
        | None -> [], []
        | Some proj ->
            let p = VsProj.Load proj projProps
            p.ItemsCompilePath, [p.OutputFilePdb]

    let cd = Directory.GetCurrentDirectory()
    let pdbs = pPdbs @ ({BaseDirectory=cd; Includes=pdbs; Excludes=[]} |> List.ofSeq)
    let projectFiles = {BaseDirectory=cd; Includes= pFiles @ files; Excludes=notFiles} |> List.ofSeq

    verbosefn "\nglobbed pdbs: %A" pdbs
    verbosefn "globbed gitFiles: %A" projectFiles

    let paths =
        if noPaths then
            use repo = new GitRepo(repoDir)
            repo.Paths projectFiles
        else paths |> Seq.ofList

    let commit =
        if commit.IsSome then commit.Value
        else
            use repo = new GitRepo(repoDir)
            repo.Commit

    for pdbPath in pdbs do
        tracefn "indexing %s" pdbPath

        let srcsrvPath =
            use pdb = new PdbFile(pdbPath)

            // verify checksums in the pdb 1st
            if verifyPdb then
                let checksums = pdb.MatchChecksums projectFiles
                if checksums.Unmatched.Count > 0 then
                    let error = sprintf "%d files do not have matching checksums in the pdb" checksums.Unmatched.Count
                    traceError error
                    for um in checksums.Unmatched do
                        traceErrorfn "  %s %s" um.Checksum um.File
                    failwith error

                // verify checksums in git 2nd
                if verifyGit then
                    let gitFiles = checksums.MatchedFiles
                    use repo = new GitRepo(repoDir)
                    tracefn "verifying checksums for %d source files in Git repository" gitFiles.Length
                    use repo = new GitRepo(repoDir)
                    let different = repo.VerifyFiles gitFiles
                    if different.Length > 0 then
                        let error = sprintf "%d files do not have matching checksums in Git" different.Length
                        traceError error
                        traceErrorfn "make sure the source code is committed and line endings match"
                        traceErrorfn "http://ctaggart.github.io/SourceLink/how-it-works.html"
                        for file in different do
                            traceErrorfn "  %s" file
                        failwith error

            pdb.PathSrcSrv

        File.WriteAllBytes(srcsrvPath, SrcSrv.create url commit paths)
        let pdbstr = Path.combine (System.Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName) "pdbstr.exe"
        
        let p = Process()
        p.FileName <- pdbstr
        p.Arguments <- sprintf "-w -s:srcsrv -i:\"%s\" -p:\"%s\"" srcsrvPath pdbPath
        p.WorkingDirectory <- cd
        p.Stdout |> Observable.add traceVerbose
        p.Stderr |> Observable.add traceError
        try
            verbosefn "%s>\"%s\" %s" p.WorkingDirectory p.FileName p.Arguments
            let exit = p.Run()
            if exit <> 0 then 
                failwithf "process failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
        with
            | ex -> 
                failwithf "process failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex