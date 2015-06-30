module SourceLink.Commands

// UnionArgParser usage based on Paket
// https://github.com/fsprojects/Paket/blob/master/src/Paket/Commands.fs

open Nessos.UnionArgParser

type Command =
    | [<First>][<CustomCommandLine "index">] Index
    | [<First>][<CustomCommandLine "checksums">] Checksums
    | [<First>][<CustomCommandLine "pdbstrr">] Pdbstrr
    | [<First>][<CustomCommandLine "srctoolx">] Srctoolx
    | [<First>][<CustomCommandLine "linefeed">] LineFeed
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Index -> "source indexes a pdb file"
            | Checksums _ -> "prints the pdb files and their checksums, supports verifying them"
            | Pdbstrr _ -> "prints the source index like `pdbstr -r -s:srcsrv`"
            | Srctoolx _ -> "lists the URLs for the soure indexed files like `srctool -x`"
            | LineFeed _ -> "update the source files to have LF line endings"

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
    | [<AltCommandLine "-nvg">] Not_Verify_Git
    | [<AltCommandLine "-nvp">] Not_Verify_Pdb
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
            | Not_Verify_Git _ -> "do not verify the file checksums using the Git repo"
            | Not_Verify_Pdb _-> "do not verify the file checksums using the pdb file"
            | Repo _ -> "Git repository directory, defaults to current directory"
            | Map _ -> "manual mapping of file path to repo path, disables verify, supports multiple"

type ChecksumsArgs =
    | [<AltCommandLine "-p">] Pdb of string
    | [<AltCommandLine "-nf">] Not_File
    | [<AltCommandLine "-u">] Url
    | [<AltCommandLine "-c">] Check
    | [<AltCommandLine "-un">] Username 
    | [<AltCommandLine "-pw">] Password
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Pdb _ -> "pdb file"
            | Not_File _ -> "do not print the source file path"
            | Url _ -> "print the download URL"
            | Check _ -> "verify the checksums by downloading and calculating in memory"
            | Username _ -> "the username for basic auth to a private repository"
            | Password _ -> "the password for basic auto to a private repository"

type PdbstrrArgs =
    | [<AltCommandLine("-p")>] Pdb of string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Pdb _ -> "pdb file"

type SrctoolxArgs =
    | [<AltCommandLine("-p")>] Pdb of string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Pdb _ -> "pdb file"

type LineFeedArgs =
    | [<AltCommandLine "-pr">] Proj of string
    | [<AltCommandLine "-pp">] Proj_Prop of string * string
    | [<AltCommandLine "-f">] File of string
    | [<AltCommandLine "-nf">] Not_File of string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Proj _ -> "project file"
            | Proj_Prop _ -> "project property, supports multiple"
            | File _ -> "source file to put in the index, supports multiple and globs"
            | Not_File _ -> "exclude this file, supports multiple and globs"

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