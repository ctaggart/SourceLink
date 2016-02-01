module SourceLink.Index

open System.IO
open SourceLink

let run (proj:string option) (projProps:(string * string) list)
    (url:string) (commit:string option)
    (pdbs:string list)
    (verifyGit:bool) (verifyPdb:bool) 
    (files:string list) (notFiles:string list)
    (repoDir:string option) 
    (paths:(string * string) list)
    (runPdbstr:bool) =
    
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
    verbosefn "globbed projectFiles: %A" projectFiles

    let repoDir =
        lazy (
            match repoDir with
            | Some v -> GitRepo.Find v
            | None -> GitRepo.Find (Directory.GetCurrentDirectory())
        )

    let paths =
        if noPaths then
            use repo = new GitRepo(repoDir.Force())
            repo.Paths projectFiles
        else paths |> Seq.ofList

    let commit =
        if commit.IsSome then commit.Value
        else
            use repo = new GitRepo(repoDir.Force())
            repo.Commit


    for pdbPath in pdbs do
       
        // verify checksums in the pdb 1st
        if verifyPdb then
            use pdb = new PdbFile(pdbPath)
            let pc = pdb.MatchChecksums projectFiles
            if pc.Unmatched.Count > 0 then
                let error = sprintf "%d files do not have matching checksums in the pdb" pc.Unmatched.Count
                traceError error
                for um in pc.Unmatched do
                    traceErrorfn "  pdb %s, file %s %s" um.ChecksumInPdb um.ChecksumOfFile um.File
                failwith error

            // verify checksums in git 2nd
            if verifyGit then
                let gitFiles = pc.MatchedFiles
                use repo = new GitRepo(repoDir.Force())
                tracefn "verifying checksums for %d source files in Git repository" gitFiles.Length
                let gc = repo.MatchChecksums gitFiles
                if gc.Unmatched.Count > 0 then
                    let error = sprintf "%d files do not have matching checksums in Git" gc.Unmatched.Count
                    traceError error
                    traceErrorfn "make sure the source code is committed and line endings match"
                    traceErrorfn "http://ctaggart.github.io/SourceLink/how-it-works.html"
                    for um in gc.Unmatched do
                        traceErrorfn "  git %s, file %s %s" um.ChecksumInGit um.ChecksumOfFile um.File
                    failwith error
        
        // do not verify the pdb
        // this is the workflow for creating indexes for the portable pdb files
        // simply emit warnings for files that do not match, not errors
        else 
            let gitFiles = projectFiles
            use repo = new GitRepo(repoDir.Force())
            tracefn "verifying checksums for %d source files in Git repository" gitFiles.Length
            let gc = repo.MatchChecksums gitFiles
            if gc.Unmatched.Count > 0 then
                let error = sprintf "%d files do not have matching checksums in Git" gc.Unmatched.Count
                traceWarn error
                traceWarnfn "make sure the source code is committed and line endings match"
                traceWarnfn "http://ctaggart.github.io/SourceLink/how-it-works.html"
                for um in gc.Unmatched do
                    traceWarnfn "  git %s, file %s %s" um.ChecksumInGit um.ChecksumOfFile um.File
        
        let srcsrvPath = pdbPath + ".srcsrv"
        tracefn "create source index %s" srcsrvPath
        File.WriteAllBytes(srcsrvPath, SrcSrv.create url commit paths)

        if runPdbstr then
            let pdbstr = Path.combine (System.Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName) "pdbstr.exe"
        
            let p = Process()
            p.FileName <- pdbstr
            p.Arguments <- sprintf "-w -s:srcsrv -i:\"%s\" -p:\"%s\"" srcsrvPath pdbPath
            p.WorkingDirectory <- cd
            p.Stdout |> Observable.add traceVerbose
            p.Stderr |> Observable.add traceError
            try
                verbosefn "%s> \"%s\" %s" p.WorkingDirectory p.FileName p.Arguments
                let exit = p.Run()
                if exit <> 0 then 
                    failwithf "process failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
            with
                | ex -> 
                    failwithf "process failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex