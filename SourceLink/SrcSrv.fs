namespace SourceLink

open System
open System.IO

module SrcSrv =
    let createTrg rawUrl (revision:string) =
        String.Format(rawUrl, revision)

    let create rawUrl (revision:string) (paths:seq<string*string>) =
        use ms = new MemoryStream()
        use sw = new StreamWriter(ms)
        let scheme = Uri(rawUrl).Scheme
        fprintfn sw "SRCSRV: ini ------------------------------------------------"
        fprintfn sw "VERSION=1"
        fprintfn sw "SRCSRV: variables ------------------------------------------"
        fprintfn sw "SRCSRVVERCTRL=%s" scheme
        fprintfn sw "SRCSRVTRG=%s" (createTrg rawUrl revision)
        fprintfn sw "SRCSRV: source files ---------------------------------------"
        for file, path in paths do
            fprintfn sw "%s*%s" file path
        fprintfn sw "SRCSRV: end ------------------------------------------------"
        sw.Flush()
        ms.ToArray()

    // TODO bug https://github.com/ctaggart/SourceLink/issues/6
    /// writes the srcsrv file to the pdb without using pdbstr.exe
    let write pdb srcsrv =
        use pdb = new PdbFile(pdb)
        pdb.FreeInfo()
        pdb.WriteSrcSrv(File.ReadAllBytes srcsrv)
        pdb.Info.Age <- pdb.Info.Age + 1
        pdb.SaveInfo()

[<AutoOpen>]
module PdbFileCreateSrcSrv =
    type PdbFile with
        member x.CreateSrcSrv repoUrl revision paths =
            File.WriteAllBytes(x.PathSrcSrv, SrcSrv.create repoUrl revision paths)