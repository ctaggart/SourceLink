module SourceLink.Program

// UnionArgParser usage base on Paket
// https://github.com/fsprojects/Paket/blob/master/src/Paket/Program.fs

open System
open Nessos.UnionArgParser
open System.Diagnostics
open System.Reflection
open System.IO
open SourceLink.Commands

let stopWatch = Stopwatch()
stopWatch.Start()

let version =
    let assembly = Assembly.GetExecutingAssembly()
    let iv = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() |> Option.ofNull |> Option.map (fun at -> at.InformationalVersion)
    match iv with
    | None -> ""
    | Some iv -> 
        let ss = iv.Split [|'\"'|]
        if ss.Length > 3 then ss.[3] else ""

tracefn "SourceLink %s" version

let filterGlobalArgs args = 
    let globalResults = 
        UnionArgParser.Create<GlobalArgs>()
            .Parse(ignoreMissing = true, 
                   ignoreUnrecognized = true, 
                   raiseOnUsage = false)
    let verbose = globalResults.Contains <@ GlobalArgs.Verbose @>
    
    let rest = 
        if verbose then 
            args |> Array.filter (fun a -> a <> "-v" && a <> "--verbose")
        else args
    
    verbose, rest

let processWithValidation<'T when 'T :> IArgParserTemplate> validateF commandF command 
    args = 
    let parser = UnionArgParser.Create<'T>()
    let results = 
        parser.Parse
            (inputs = args, raiseOnUsage = false, ignoreMissing = true, 
             errorHandler = ProcessExiter())
    let resultsValid = validateF (results)
    if results.IsUsageRequested || not resultsValid then 
        parser.Usage(Commands.cmdLineUsageMessage command parser) |> trace
    else 
        commandF results
        if Logging.verbose then
            let elapsedTime = Utils.TimeSpanToReadableString stopWatch.Elapsed
            tracefn "elapsed time: %s" elapsedTime

let processCommand<'T when 'T :> IArgParserTemplate> (commandF : ArgParseResults<'T> -> unit) = 
    processWithValidation (fun _ -> true) commandF 

let v, args = filterGlobalArgs (Environment.GetCommandLineArgs().[1..])
Logging.verbose <- v

let index (results: ArgParseResults<_>) =
    let proj = results.TryGetResult <@ IndexArgs.Proj @>
    let projProps = results.GetResults <@ IndexArgs.Proj_Prop @>
    let url = results.GetResult <@ IndexArgs.Url @>
    let commit = results.TryGetResult <@ IndexArgs.Commit @>
    let pdbs = results.GetResults <@ IndexArgs.Pdb @>
    let verifyGit = defaultArg (results.TryGetResult <@ IndexArgs.Verify_Git @>) true
    let verifyPdb = defaultArg (results.TryGetResult <@ IndexArgs.Verify_Pdb @>) true
    let files = results.GetResults <@ IndexArgs.File @>
    let notFiles = results.GetResults <@ IndexArgs.Not_File @>
    let repoDir = defaultArg (results.TryGetResult <@ IndexArgs.Repo @>) (Directory.GetCurrentDirectory())
    let paths = results.GetResults <@ IndexArgs.Map @>
    IndexCmd.run proj projProps url commit pdbs verifyGit verifyPdb files notFiles repoDir paths

let srctoolx (results: ArgParseResults<_>) =
    let pdb = results.GetResult <@ SrcToolxArgs.Pdb @>
    SrcToolx.run pdb

let checksums (results: ArgParseResults<_>) =
    let pdb = results.GetResult <@ ChecksumsArgs.Pdb @>
    let file = defaultArg (results.TryGetResult <@ ChecksumsArgs.File @>) true
    let url = defaultArg (results.TryGetResult <@ ChecksumsArgs.Url @>) false
    let check = defaultArg (results.TryGetResult <@ ChecksumsArgs.Check @>) false
    Checksums.run pdb file url check

try
    let parser = UnionArgParser.Create<Command>()
    let results = 
        parser.Parse(inputs = args,
                   ignoreMissing = true, 
                   ignoreUnrecognized = true, 
                   raiseOnUsage = false)

    match results.GetAllResults() with
    | [ command ] -> 
        let handler =
            match command with
            | Index -> processCommand index
            | SrcToolx -> processCommand srctoolx
            | Checksums -> processCommand checksums

        let args = args.[1..]
        handler command args
    | [] -> 
        parser.Usage("available commands:") |> trace
    | _ -> failwith "expected only one command"
with
| exn when not (exn :? System.NullReferenceException) -> 
    Environment.ExitCode <- 1
    traceErrorfn "SourceLink failed with:%s  %s" Environment.NewLine exn.Message

    if verbose then
        traceErrorfn "StackTrace:%s  %s" Environment.NewLine exn.StackTrace
