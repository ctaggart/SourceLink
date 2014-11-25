// thanks Paket! derived from https://github.com/fsprojects/Paket/blob/master/src/Paket/Program.fs
module SourceLink.Program

open System
open Nessos.UnionArgParser
open System.Diagnostics
open System.Reflection
open System.IO
open SourceLink

//let private stopWatch = new Stopwatch()
//stopWatch.Start()

let version =
    let assembly = Assembly.GetExecutingAssembly()
    let iv = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() |> Option.ofNull |> Option.map (fun at -> at.InformationalVersion)
    match iv with
    | None -> ""
    | Some iv -> 
        let ss = iv.Split [|'\"'|]
        if ss.Length > 3 then ss.[3] else ""

tracefn "SourceLink %s" version

type Command =
    | Index
    | Unknown

type CLIArguments =
    | [<First>][<NoAppSettings>][<CustomCommandLine("index")>] Index
    | [<AltCommandLine("-v")>] Verbose
    | [<AltCommandLine("-p")>] Proj of string
    | [<AltCommandLine("-pp")>] Proj_Prop of string * string
    | [<AltCommandLine("-u")>] Url of string
    | Commit of string
    | Pdb of string
    | [<AltCommandLine("-f")>] File of string
    | [<AltCommandLine("-nf")>] Not_File of string
    | Verify_Git of bool
    | Verify_Pdb of bool
    | [<AltCommandLine("-r")>] Repo of string
    | [<AltCommandLine("-m")>] Map of string * string

with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Index -> "source indexes a pdb file"

            | Verbose -> "displays verbose output"

            | Proj _ -> "project file path"
            | Proj_Prop _ -> "project property, supports multiple"
            | Url _ -> "URL for downloading the source files, use {0} for commit and %var2% for path"
            | Commit _ -> "Git commit or Hg changeset ID or SVN revision number or TFVC changeset ID"
            | Pdb _ -> "pdb file to add the index to, supports multiple and globs"
            | File _ -> "source file to put in the index, supports multiple and globs"
            | Not_File _ -> "exclude this file, supports multiple and globs"
            | Verify_Git _ -> "verify the file checksums using the Git repo, default true"
            | Verify_Pdb _-> "verify the file checksums using the pdb file, default true"
            | Repo _ -> "Git repository directory, defaults to current directory"
            | Map _ -> "manual mapping of file path to repo path, disables verify, supports multiple"

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
            let url = results.GetResult <@ CLIArguments.Url @>
            let commit = results.TryGetResult <@ CLIArguments.Commit @>
            let pdbs = results.GetResults <@ CLIArguments.Pdb @>
            let verifyGit = defaultArg (results.TryGetResult <@ CLIArguments.Verify_Git @>) true
            let verifyPdb = defaultArg (results.TryGetResult <@ CLIArguments.Verify_Pdb @>) true
            let files = results.GetResults <@ CLIArguments.File @>
            let notFiles = results.GetResults <@ CLIArguments.Not_File @>
            let repoDir = defaultArg (results.TryGetResult <@ CLIArguments.Repo @>) (Directory.GetCurrentDirectory())
            let paths = results.GetResults <@ CLIArguments.Map @>
            IndexCmd.run proj projProps url commit pdbs verifyGit verifyPdb files notFiles repoDir paths

        | _ -> traceErrorfn "no command given.%s" (parser.Usage())
        
//        let elapsedTime = Utils.TimeSpanToReadableString stopWatch.Elapsed
//        tracefn "%s - ready." elapsedTime
    | None -> ()
with
| ex -> 
    Environment.ExitCode <- 1
    traceErrorfn "SourceLink failed with:%s  %s" Environment.NewLine ex.Message
    if verbose then
        traceErrorfn "StackTrace:%s  %s" Environment.NewLine ex.StackTrace
