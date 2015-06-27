namespace SourceLink

open System
open System.IO

module SrcSrv =
    let createTrg rawUrl (commit:string) =
        String.Format(rawUrl, commit)

    let noFormatting (s: string) = s

    /// creates the SrcSrv with callback for formatting the path
    /// paths is the list of original file system paths and their repository paths
    let createFormat rawUrl (commit:string) (paths:seq<string*string>) (formatPath: string -> string) =
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
            fprintfn sw "%s*%s" file (formatPath path)
        fprintfn sw "SRCSRV: end ------------------------------------------------"
        sw.Flush()
        ms.ToArray()

    let create rawUrl commit paths =
        createFormat rawUrl commit paths noFormatting

    /// create the SrcSrv with the paths escaped using Uri.EscapeDataString
    let createEscaped rawUrl commit paths =
        createFormat rawUrl commit paths Uri.EscapeDataString

[<AutoOpen>]
module PdbFileCreateSrcSrv =
    type PdbFile with

        /// create the SrcSrv
        member x.CreateSrcSrv repoUrl commit paths =
            File.WriteAllBytes(x.PathSrcSrv, SrcSrv.create repoUrl commit paths)

        /// create the SrcSrv with the paths escaped using Uri.EscapeDataString
        member x.CreateSrcSrvEscaped repoUrl commit paths =
            File.WriteAllBytes(x.PathSrcSrv, SrcSrv.create repoUrl commit paths)