module SourceLink.Extension

open System
open System.Globalization
open System.Collections.Generic

let private zulu (dt:DateTime) (fmt:string) =
    let s = dt.ToString fmt
    if dt.Kind = DateTimeKind.Utc then s + "Z" else s

type DateTime with
    member x.IsoDateTime with get() = zulu x DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern
    member x.IsoDate with get() = zulu x "yyyy'-'MM'-'dd"

type Guid with
    member x.ToStringN with get() = x.ToString("N").ToUpperInvariant()

type String with
    member x.ToUtf8 with get() = Text.Encoding.UTF8.GetBytes x

type ICollection<'T> with
    // similar to linq SequenceEquals
    member a.CollectionEquals(b:ICollection<'T>) =
        if a.Count <> b.Count then
            false
        else
            let comparer a' b' = Comparer<'T>.Default.Compare(a', b')
            (Seq.compareWith comparer a b) = 0
