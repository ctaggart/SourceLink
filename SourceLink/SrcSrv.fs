module SourceLink.SrcSrv

open System
open System.IO
open System.Collections.Generic
open SourceLink
open SourceLink.File

let createSrcSrvTrg urlBase (revision:string) =
    String.Format(urlBase, revision)

let createSrcSrv urlBase (revision:string) (sourceFiles:IList<string*string>) =
    use ms = new MemoryStream()
    use sw = new StreamWriter(ms)
    let scheme = Uri(urlBase).Scheme
    fprintfn sw "SRCSRV: ini ------------------------------------------------"
    fprintfn sw "VERSION=1"
    fprintfn sw "SRCSRV: variables ------------------------------------------"
    fprintfn sw "SRCSRVVERCTRL=%s" scheme
    fprintfn sw "SRCSRVTRG=%s" (String.Format(urlBase, revision))
    fprintfn sw "SRCSRV: source files ---------------------------------------"
    for file, path in sourceFiles do
        fprintfn sw "%s*%s" file path
    fprintfn sw "SRCSRV: end ------------------------------------------------"
    sw.Flush()
    ms.ToArray()


