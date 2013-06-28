module SourceLink.SrcSrv

open System
open System.IO
open System.Collections.Generic
open SourceLink.Pdb

let pathCombine a b = Path.Combine(a, b)

let writeSrcSrv (pdb:PdbInfo) (dirSrc:string) filesSrc urlBase dirSrcSrv revision =
    let checksumsSrc = computeChecksums filesSrc

    let map = SortedDictionary(StringComparer.OrdinalIgnoreCase)
    let missingFiles = SortedDictionary(StringComparer.OrdinalIgnoreCase)
    let found = ref 0
    for KeyValue(checksum, file) in pdb.Checksums do
        if checksumsSrc.ContainsKey checksum then
            let path = checksumsSrc.[checksum].Substring(dirSrc.Length+1).Replace('\\','/')
            map.Add(file, path)
            incr found
        else
            missingFiles.[file] <- checksum

    if missingFiles.Count > 0 then
        printfn "found %d files" !found
        printfn "could not find %d files" missingFiles.Count
        for KeyValue(file, hash) in missingFiles do
            printfn "%s %s" hash file
        failwithf "could not find %d files" missingFiles.Count

    let fn = Path.GetFileName pdb.File
    let dirTxt = pathCombine dirSrcSrv (pathCombine fn pdb.PeId)
    Directory.CreateDirectory dirTxt |> ignore
    let txt = pathCombine dirTxt (fn + ".srcsrv.txt")
    printfn "writing %s" txt

    use sw = new StreamWriter(txt)
    let scheme = Uri(urlBase).Scheme
    fprintfn sw "SRCSRV: ini ------------------------------------------------"
    fprintfn sw "VERSION=1"
    fprintfn sw "SRCSRV: variables ------------------------------------------"
    fprintfn sw "SRCSRVVERCTRL=%s" scheme
    fprintfn sw "SRCSRVTRG=%s/%s/%%var2%%" (urlBase.TrimEnd [|'/'|]) revision
    fprintfn sw "SRCSRV: source files ---------------------------------------"
    for (KeyValue(file, path)) in map do
        fprintfn sw "%s*%s" file path
    fprintfn sw "SRCSRV: end ------------------------------------------------"

    let pdbAge = pathCombine dirTxt (Path.GetFileNameWithoutExtension(pdb.File) + "." + pdb.Age.ToString() + ".pdb")
    File.Copy(pdb.File, pdbAge)