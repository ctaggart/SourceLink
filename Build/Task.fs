namespace SourceLink.Build

open Microsoft.Build.Framework

[<AbstractClass>]
type Task() =
    inherit Microsoft.Build.Utilities.Task()

    // log messages with F# printf formatting
    member private x.LogMessage importance message = base.Log.LogMessage(importance, message, [||])
    member private x.LogError message = base.Log.LogError(message, [||])
    
    /// logs a message during a build just like Message Task
    member x.Message importance format = Printf.ksprintf (fun message -> x.LogMessage importance message) format
    member x.MessageHigh format = x.Message MessageImportance.High format
    member x.MessageNormal format = x.Message MessageImportance.Normal format
    member x.MessageLogLow format = x.Message MessageImportance.Low format
    
    /// logs an error and potentially fails the build
    member x.Error format = Printf.ksprintf (fun message -> x.LogError message) format
    member x.HasErrors with get() = x.Log.HasLoggedErrors