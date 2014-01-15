[<AutoOpen>]
module SourceLink.SrcSrv

open System
open System.IO

let createSrcSrvTrg urlBase (revision:string) =
    String.Format(urlBase, revision)

type PdbFile with
    // http://blog.ctaggart.com/2013/07/source-linking.html
    static member CreateSrcSrv rawUrl (revision:string) (paths:seq<string*string>) =
        use ms = new MemoryStream()
        use sw = new StreamWriter(ms)
        let scheme = Uri(rawUrl).Scheme
        fprintfn sw "SRCSRV: ini ------------------------------------------------"
        fprintfn sw "VERSION=1"
        fprintfn sw "SRCSRV: variables ------------------------------------------"
        fprintfn sw "SRCSRVVERCTRL=%s" scheme
        fprintfn sw "SRCSRVTRG=%s" (String.Format(rawUrl, revision))
        fprintfn sw "SRCSRV: source files ---------------------------------------"
        for file, path in paths do
            fprintfn sw "%s*%s" file path
        fprintfn sw "SRCSRV: end ------------------------------------------------"
        sw.Flush()
        ms.ToArray()

    member x.WriteSrcSrvToFile repoUrl revision paths =
        File.WriteAllBytes(x.PathSrcSrv, PdbFile.CreateSrcSrv repoUrl revision paths)

    member x.SetSrcSrv() =
        // TODO use pdbstr
        x.FreeInfo()
        File.ReadAllBytes x.PathSrcSrv |> x.WriteSrcSrv
        x.Info.Age <- x.Info.Age + 1
        x.SaveInfo()