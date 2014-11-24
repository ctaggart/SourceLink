// thanks Paket! derived from https://github.com/fsprojects/Paket/blob/master/src/Paket/Program.fs
module SourceLink.Program

open System
open Nessos.UnionArgParser
open System.Diagnostics
open System.Reflection
open System.IO

let private stopWatch = new Stopwatch()
stopWatch.Start()

let assembly = Assembly.GetExecutingAssembly()
let fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
tracefn "SourceLink version %s" fvi.FileVersion

type Command =
    | Index
    | Unknown

type CLIArguments =
    | [<First>][<NoAppSettings>][<CustomCommandLine("index")>] Index
    | [<AltCommandLine("-v")>] Verbose
    | Proj of string
    | Proj_Prop of string * string
    | Url of string
    | Commit of string
    | Pdb of string
    | Files of string
    | Verify_Git of bool
    | Verify_Pdb of bool

with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Index -> "source indexes a pdb file"

            | Verbose -> "displays verbose output"

            | Proj _ -> "project file path"
            | Proj_Prop _ -> "project property, supports multiple"
            | Url _ -> "URL for downloading the source files, use {0} for commit and %var2% for path placeholders"
            | Commit _ -> "commit "
            | Pdb _ -> "pdb file to source index, supports multiple"
            | Files _ -> "file pattern for files to include, supports multiple"
            | Verify_Git _ -> "verify the file checksums using the Git repo, default true"
            | Verify_Pdb _-> "verify the file checksums using the pdb file, default true"

let parser = UnionArgParser.Create<CLIArguments>("USAGE: sourcelink [index] ... options")
 
let results =
    try
        let results = parser.Parse()
        let command = 
            if results.Contains <@ CLIArguments.Index @> then Command.Index
            else Command.Unknown
        if results.Contains <@ CLIArguments.Verbose @> then
            verbose <- true

        Some(command,results)
    with
    | _ ->
        tracefn "%s %s%s" (String.Join(" ",Environment.GetCommandLineArgs())) Environment.NewLine (parser.Usage())
        None

try
    match results with
    | Some(command,results) ->
        
        match command with
        | Command.Index ->
            let proj = results.TryGetResult <@ CLIArguments.Proj @>
            let projProps = results.GetResults <@ CLIArguments.Proj_Prop @>
            let url = results.TryGetResult <@ CLIArguments.Url @>
            let commit = results.TryGetResult <@ CLIArguments.Commit @>
            let pdbs = results.GetResults <@ CLIArguments.Pdb @>
            let verifyGit = results.GetResult <@ CLIArguments.Verify_Git @>
            let verifyPdb = results.GetResult <@ CLIArguments.Verify_Pdb @>

            IndexCmd.run proj projProps url commit pdbs verifyGit verifyPdb
            
            ()
        | _ -> traceErrorfn "no command given.%s" (parser.Usage())
        
        let elapsedTime = Utils.TimeSpanToReadableString stopWatch.Elapsed

        tracefn "%s - ready." elapsedTime
    | None -> ()
with
| exn -> 
    Environment.ExitCode <- 1
    traceErrorfn "sourcelink failed with:%s   %s" Environment.NewLine exn.Message

    if verbose then
        traceErrorfn "StackTrace:%s  %s" Environment.NewLine exn.StackTrace
