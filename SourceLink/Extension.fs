module SourceLink.Extension

open System
open System.Globalization
open System.Collections.Generic
open System.IO
open System.Text

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

type BinaryReader with
    member x.ReadGuid() = Guid(x.ReadBytes 16)
    member x.ReadCString() =
        let byte = ref 0uy
        byte := x.ReadByte()
        seq {
            while !byte <> 0uy do
                yield !byte
                byte := x.ReadByte()
        }
        |> Seq.toArray
        |> Text.Encoding.UTF8.GetString
    member x.Position 
        with get() = int x.BaseStream.Position 
        and set(i:int) = x.BaseStream.Position <- int64 i
    member x.Skip i = x.Position <- x.Position + i

type BinaryWriter with
    member x.WriteGuid (guid:Guid) = x.Write (guid.ToByteArray())

type StringBuilder with
    member x.Appendf format = Printf.ksprintf (fun s -> x.Append s |> ignore) format