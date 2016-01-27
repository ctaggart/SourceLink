namespace SourceLink

open Argu
open SourceLink
open SourceLink.Commands
open System
open System.IO
open System.Text
open Microsoft.Build.Framework
open System.Collections.Generic

type SourceLinkTask() =
    inherit Task()

    [<Required>]
    member val Url = String.Empty with set, get

    [<Required>]
    member val ProjectDirectory = String.Empty with set, get

    [<Required>]
    member val Sources = Array.empty<string> with set, get

    [<Required>]
    member val TargetPath = String.Empty with set, get

    override x.Execute() =
        let parser = ArgumentParser.Create<IndexArgs>()
        let args =
            [
                yield IndexArgs.Not_Verify_Pdb
                yield IndexArgs.Not_Pdbstr
                yield IndexArgs.Pdb (Path.ChangeExtension(x.TargetPath, ".pdb"))
                yield IndexArgs.Url x.Url
                for source in x.Sources do
                    yield IndexArgs.File source
            ]
            |> parser.PrintCommandLine
            
        let exe = Path.combine (System.Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName) "SourceLink.exe"

        let psSpecialChars = [| '%' |] // PowerShell special characters
        let psNeedsQuotes (s: string) = s.IndexOfAny psSpecialChars <> -1
        let arguments =
            let sb = StringBuilder()
            sb.Appendf "index"
            for arg in args do
                if psNeedsQuotes arg then
                    sb.Appendf " '%s'" arg
                else sb.Appendf " %s" arg
            sb.ToString()

        let p = Process()
        p.FileName <- exe
        p.Arguments <-arguments
        p.WorkingDirectory <- x.ProjectDirectory // x.Sources are relative
        let out = StringBuilder()
        p.Stdout |> Observable.add (out.Appendf "%s\n")
        p.Stderr |> Observable.add (out.Appendf "%s\n")
        let cmd = sprintf "%s> . '%s' %s" p.WorkingDirectory p.FileName p.Arguments
        x.MessageNormal "%s" cmd
        try
            let exit = p.Run()
            if exit <> 0 then 
                x.MessageHigh "SourceLink failed:"
                x.MessageHigh "%s" cmd
                x.MessageHigh "%s" (out.ToString())
                x.Error "SourceLink failed. See build output for details."
            else
                x.MessageNormal "%s" (out.ToString())
        with
            | ex -> 
                x.MessageHigh "SourceLink failed: %s" ex.Message
                x.MessageHigh "%s" cmd
                x.MessageHigh "%s" (out.ToString())
                x.Error "SourceLink failed. See build output for details."
        not x.HasErrors