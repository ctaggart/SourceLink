module SourceLink.Checksums

open System
open System.Text
open System.Collections.Generic
open System.Net.Http
open SourceLink.Http
open System.Security.Cryptography

type CheckResponse =
    | Pass
    | Fail of string

let checkUrl (hc: HttpClient) url checksum = async {
    use ha = MD5.Create() // not thread safe, get `Hash not valid for use in specified state.`
    use req = new HttpRequestMessage(HttpMethod.Get, Uri url)
    use! rsp = hc.Send req
    if rsp.IsSuccessStatusCode then
        use! file = rsp.Content.ReadAsStream()
        let checksumUrl = ha.ComputeHash file
        let hexChecksum = Hex.encode checksum
        let hexChecksumUrl = Hex.encode checksumUrl
        if hexChecksum = hexChecksumUrl then
            return Pass
        else return Fail hexChecksumUrl 
    else return Fail (sprintf "HTTP %d" (int rsp.StatusCode)) }

let run (pdb: string) showFiles showUrls check =
    let urls =
        if showUrls || check then
            SrcToolx.getSourceFilePathAndUrl pdb
            |> Seq.filter (fun (sf, url) -> url.IsSome)
            |> Seq.map (fun (sf, url) -> sf, url.Value)
            |> Dictionary.ofTuplesCmp StringComparer.OrdinalIgnoreCase
        else Dictionary()

    use hc = if check then createHttpClient() else Unchecked.defaultof<_>
    
    let p = new PdbFile(pdb)
    let nFiles = ref 0
    let nUrls = ref 0
    let nPass = ref 0
    let nFail = ref 0

    // TODO It would be great if this could do this in parallel.
    //      I would want the order to be preserved and progress to be printed.
    //      One problem I encountered was that nFiles ended up being one less than the correct total.
    p.Files |> Seq.iter (fun (file, checksum) ->
//    p.Files |> Seq.map (fun (file, checksum) -> async {
        incr nFiles
        let sb = StringBuilder()
        sb.Appendf "%s" (Hex.encode checksum)
        if showFiles then
            sb.Appendf " %s" file
        if urls.ContainsKey file then
            let url = urls.[file]
            if showUrls then
                sb.Appendf " %s " url
            if check then
                let checkRsp = checkUrl hc url checksum |> Async.RunSynchronously
//                let! checkRsp = checkUrl hc url checksum 
                match checkRsp with
                | Pass -> incr nPass
                | Fail err -> incr nFail; sb.Appendf " FAIL %s" err
        printfn "%s" (sb.ToString())
    )
//        return sb.ToString()
//    })
//    |> Async.Parallel
//    |> Async.RunSynchronously
//    |> Seq.iter (printfn "%s")

    let sb = StringBuilder()
    sb.Appendf "%s has %d source files" pdb !nFiles
    if showUrls then
        if !nFiles = !nUrls then sb.Appendf ", ALL indexed"
        else sb.Appendf ", %d indexed, %d not indexed" !nUrls (!nFiles - !nUrls)
    if check then
        if !nFiles = !nPass then sb.Appendf ", ALL passed"
        else sb.Appendf ", %d passed, %d failed" !nPass !nFail
    printfn "%s" (sb.ToString())