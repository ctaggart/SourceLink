namespace SourceLink

open System

type Process() =
    let si = Diagnostics.ProcessStartInfo()
    do
        si.UseShellExecute <- false
        si.CreateNoWindow <- true
        si.RedirectStandardOutput <- true
        si.RedirectStandardError <- true
    let stdout = Event<string>()
    let stderr = Event<string>()
    member x.FileName with set v = si.FileName <- v and get() = si.FileName
    member x.Arguments with set v = si.Arguments <- v and get() = si.Arguments
    member x.WorkingDirectory with set v = si.WorkingDirectory <- v and get() = si.WorkingDirectory
    member x.Stdout = stdout.Publish
    member x.Stderr = stderr.Publish
    member x.Run() =
        use p = new Diagnostics.Process()
        p.StartInfo <- si
        p.OutputDataReceived |> Event.add(fun ev -> if ev.Data <> null then stdout.Trigger ev.Data)
        p.ErrorDataReceived.Add(fun ev -> if ev.Data <> null then stderr.Trigger ev.Data)
        p.Start() |> ignore
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()
        p.WaitForExit()
        p.ExitCode
