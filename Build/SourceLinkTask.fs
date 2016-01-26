namespace SourceLink

open Argu
open SourceLink
open SourceLink.Commands
open System
open System.IO
open Microsoft.Build.Framework
open System.Collections.Generic

type SourceLinkTask() =
    inherit Task()

    [<Required>]
    member val Sources = Array.empty<string> with set, get

    [<Required>]
    member val TargetPath = String.Empty with set, get

    override x.Execute() =
        try
            let parser = ArgumentParser.Create<IndexArgs>()
            let args =
                [
                    yield IndexArgs.Pdb (sprintf "%s.pdb" (Path.GetFileNameWithoutExtension x.TargetPath))
                    yield IndexArgs.No_Pdbstr
                    yield IndexArgs.Not_Verify_Pdb
                    for source in x.Sources do
                        yield IndexArgs.File source
                ]
                |> parser.PrintCommandLine
            
            let exe = Path.combine (System.Reflection.Assembly.GetExecutingAssembly().Location |> Path.GetDirectoryName) "SourceLink.exe"
        
            let p = Process()
            p.FileName <- exe
            p.Arguments <- sprintf "index %s" (String.Join(" ", args))
            p.WorkingDirectory <- Path.GetDirectoryName x.TargetPath
            p.Stdout |> Observable.add (x.MessageLow "%s")
            p.Stderr |> Observable.add (x.Error "%s")
            try
                x.MessageHigh "%s> \"%s\" %s" p.WorkingDirectory p.FileName p.Arguments // 
                let exit = p.Run()
                if exit <> 0 then 
                    x.Error "process failed with exit code %d, run '%s', with '%s', in '%s'" exit p.FileName p.Arguments p.WorkingDirectory
            with
                | ex -> 
                    x.Error "process failed with exception, run '%s', with '%s', in '%s', %A" p.FileName p.Arguments p.WorkingDirectory ex
            
        with
        | :? SourceLinkException as ex -> x.Error "%s" ex.Message
        not x.HasErrors