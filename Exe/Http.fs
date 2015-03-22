module SourceLink.Http

open System
open System.IO
open System.Net
open System.Net.Http

let createHttpClient() =
    let handler = new HttpClientHandler()
    if handler.SupportsAutomaticDecompression then
        handler.AutomaticDecompression <- DecompressionMethods.GZip ||| DecompressionMethods.Deflate
    new HttpClient(handler)

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
             
    type HttpContent with
        member x.ReadAsString() = async {
            return! x.ReadAsStringAsync() |> Async.AwaitTaskOne }
        member x.ReadAsStream() = async {
            return! x.ReadAsStreamAsync() |> Async.AwaitTaskOne }
        member x.ReadAsByteArray() = async {
            return! x.ReadAsByteArrayAsync() |> Async.AwaitTaskOne }
