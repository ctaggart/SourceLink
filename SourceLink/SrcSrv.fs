namespace SourceLink

open System
open System.IO

module SrcSrv =
    let createTrg rawUrl (commit:string) =
        String.Format(rawUrl, commit)

    let create rawUrl (commit:string) (paths:seq<string*string>) =
        use ms = new MemoryStream()
        use sw = new StreamWriter(ms)
        let scheme = Uri(rawUrl).Scheme
        fprintfn sw "SRCSRV: ini ------------------------------------------------"
        fprintfn sw "VERSION=2"
        fprintfn sw "SRCSRV: variables ------------------------------------------"
        fprintfn sw "SRCSRVVERCTRL=%s" scheme
        fprintfn sw "SRCSRVTRG=%s" (createTrg rawUrl commit)
        fprintfn sw "SRCSRV: source files ---------------------------------------"
        for file, path in paths do
            fprintfn sw "%s*%s" file path
        fprintfn sw "SRCSRV: end ------------------------------------------------"
        sw.Flush()
        ms.ToArray()


[<AutoOpen>]
module PdbFileCreateSrcSrv =
    type PdbFile with
        member x.CreateSrcSrv repoUrl commit paths =
            File.WriteAllBytes(x.PathSrcSrv, SrcSrv.create repoUrl commit paths)