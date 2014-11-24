// thanks Paket! copied subset of https://github.com/fsprojects/Paket/blob/master/src/Paket.Core/Utils.fs
[<AutoOpen>]
/// Contains methods for IO.
module SourceLink.Utils

open System
open System.IO
open System.Net
open System.Text

let TimeSpanToReadableString(span:TimeSpan) =
    let pluralize x = if x = 1 then String.Empty else "s"
    let notZero x y = if x > 0 then y else String.Empty
    let days = notZero (span.Duration().Days)  <| String.Format("{0:0} day{1}, ", span.Days, pluralize span.Days)
    let hours = notZero (span.Duration().Hours) <| String.Format("{0:0} hour{1}, ", span.Hours, pluralize span.Hours) 
    let minutes = notZero (span.Duration().Minutes) <| String.Format("{0:0} minute{1}, ", span.Minutes, pluralize span.Minutes)
    let seconds = notZero (span.Duration().Seconds) <| String.Format("{0:0} second{1}", span.Seconds, pluralize span.Seconds) 

    let formatted = String.Format("{0}{1}{2}{3}", days, hours, minutes, seconds)

    let formatted = if formatted.EndsWith(", ") then formatted.Substring(0, formatted.Length - 2) else formatted

    if String.IsNullOrEmpty(formatted) then "0 seconds" else formatted
