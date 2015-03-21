module SourceLink.Commands

// UnionArgParser usage base on Paket
// https://github.com/fsprojects/Paket/blob/master/src/Paket/Commands.fs

open Nessos.UnionArgParser

type Command =
    | [<First>][<CustomCommandLine "index">] Index
    | [<First>][<CustomCommandLine "srctoolx">] SrcToolx
    | [<First>][<CustomCommandLine "checksums">] Checksums
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Index -> "source indexes a pdb file"
            | SrcToolx _ -> "lists the URLs for the soure indexed files like `SrcTool -x`"
            | Checksums _ -> "lists all the files in the pdb and their checksums"

    member this.Name = 
        let uci,_ = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(this, typeof<Command>)
        (uci.GetCustomAttributes(typeof<CustomCommandLineAttribute>) 
        |> Seq.head 
        :?> CustomCommandLineAttribute).Name

type GlobalArgs =
    | [<AltCommandLine("-v")>] Verbose
with
    interface IArgParserTemplate with
        member __.Usage = ""

type IndexArgs =
    | [<AltCommandLine "-pr">] Proj of string
    | [<AltCommandLine "-pp">] Proj_Prop of string * string
    | [<AltCommandLine "-u">] Url of string
    | [<AltCommandLine "-c">] Commit of string
    | [<AltCommandLine "-p">] Pdb of string
    | [<AltCommandLine "-f">] File of string
    | [<AltCommandLine "-nf">] Not_File of string
    | [<AltCommandLine "-vg">] Verify_Git of bool
    | [<AltCommandLine "-vp">] Verify_Pdb of bool
    | [<AltCommandLine "-r">] Repo of string
    | [<AltCommandLine "-m">] Map of string * string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Proj _ -> "project file"
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

type SrcToolxArgs =
    | [<AltCommandLine("-p")>] Pdb of string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Pdb _ -> "pdb file"

type ChecksumsArgs =
    | [<AltCommandLine "-p">] Pdb of string
    | [<AltCommandLine "-f">] File of bool
    | [<AltCommandLine "-u">] Url of bool
    | [<AltCommandLine "-c">] Check of bool
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Pdb _ -> "pdb file"
            | File _ -> "print the source file path, default true"
            | Url _ -> "print the download URL, default false"
            | Check _ -> "check the checksums by downloading and calculating in memory"

let cmdLineSyntax (parser:UnionArgParser<_>) commandName = 
    "$ SourceLink " + commandName + " " + parser.PrintCommandLineSyntax()

let cmdLineUsageMessage (command : Command) parser =
    System.Text.StringBuilder()
        .Append("SourceLink ")
        .AppendLine(command.Name)
        .AppendLine()
        .AppendLine((command :> IArgParserTemplate).Usage)
        .AppendLine()
        .Append(cmdLineSyntax parser command.Name)
        .ToString()