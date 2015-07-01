module SourceLink.Http

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers

let createHttpClient() =
    let handler = new HttpClientHandler()
    if handler.SupportsAutomaticDecompression then
        handler.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
    let hc = new HttpClient(handler)
    hc.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue("SourceLink", version))
    hc

let base64Encode (s:string) = 
    s |> Text.Encoding.UTF8.GetBytes |> Convert.ToBase64String

[<RequireQualifiedAccess>]
module Async =
    /// raise the InnerException instead of AggregateException if there is just one
    let AwaitTaskOne task = async {
        try
            return! Async.AwaitTask task
        with e ->
            return
                match e with
                | :? AggregateException as ae ->
                    if ae.InnerExceptions.Count = 1 then raise ae.InnerException
                    else raise ae
                | _ -> raise e }

[<AutoOpen>]
module HttpExt = 
    type HttpClient with
        member x.Send req = async {
            return! x.SendAsync req |> Async.AwaitTaskOne }

        member x.SetBasicAuth (username: string option) (password: string option) =
            match username with
            | None -> ()
            | Some un ->
                let basic =
                    match password with
                    | None -> sprintf "%s:" un
                    | Some pw -> sprintf "%s:%s" un pw
                x.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", base64Encode basic)
             
    type HttpContent with
        member x.ReadAsString() = async {
            return! x.ReadAsStringAsync() |> Async.AwaitTaskOne }
        member x.ReadAsStream() = async {
            return! x.ReadAsStreamAsync() |> Async.AwaitTaskOne }
        member x.ReadAsByteArray() = async {
            return! x.ReadAsByteArrayAsync() |> Async.AwaitTaskOne }
