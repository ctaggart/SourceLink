[<AutoOpen>]
module SourceLink.SystemExtensions

open System
open System.Globalization
open System.Collections.Generic
open System.IO
open System.Text

type String with
    member x.ToUtf8 with get() = Text.Encoding.UTF8.GetBytes x
    member x.EqualsI b = x.Equals(b, StringComparison.OrdinalIgnoreCase)
    member x.UpperI with get() = x.ToUpperInvariant()
    member x.LowerI with get() = x.ToLowerInvariant()

    // 2005-05 New Recommendations for Using Strings in Microsoft .NET 2.0 http://msdn.microsoft.com/en-us/library/ms973919.aspx
    /// string comparison, similar to strcmp
    static member cmp a b = StringComparer.Ordinal.Compare(a, b)
    /// string comparison ignoring case, similar to strcmpi
    static member cmpi a b = StringComparer.OrdinalIgnoreCase.Compare(a, b)

type Uri with
    /// create a Uri from a string, allows piping
    static member from s = Uri s

let private zulu (dt:DateTime) (fmt:string) =
    let s = dt.ToString fmt
    if dt.Kind = DateTimeKind.Utc then s + "Z" else s

type DateTime with
    member x.IsoDateTime with get() = zulu x DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern
    member x.IsoDate with get() = zulu x "yyyy'-'MM'-'dd"

type Guid with
    member x.ToStringN with get() = x.ToString "N"

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

type StreamReader with
    static member ReadLines (bytes:byte[]) =
        use sr = new StreamReader(new MemoryStream(bytes))
        seq {
            while not sr.EndOfStream do
                yield sr.ReadLine()
        }
        |> Seq.toArray

type BinaryWriter with
    member x.WriteGuid (guid:Guid) = x.Write (guid.ToByteArray())

type StringBuilder with
    member x.Appendf format = Printf.ksprintf (fun s -> x.Append s |> ignore) format
    member x.String with get() = x.ToString()

type FileStream with
    member x.WriteBytes (bytes:byte[]) = x.Write(bytes, 0, bytes.Length)
    member x.WriteBytesAt bytes (position:int) = x.Position <- int64 position; x.WriteBytes bytes

type IDictionary<'K,'V> with
    member x.Get key =
        let mutable value = Unchecked.defaultof<'V>
        if x.TryGetValue(key, &value) then value |> Some else None
    member x.Set key (value:option<'V>) =
        match value with
        | Some v -> x.[key] <- v
        | None -> x.Remove key |> ignore
    member x.KeyValues = x |> Seq.map (fun pair -> pair.Key, pair.Value)
    member x.AddAll keyValues = keyValues |> Seq.iter (fun (KeyValue(k,v)) -> x.[k] <- v)
    member x.AddAll tuples = tuples |> Seq.iter (fun (k,v) -> x.[k] <- v)

type Dictionary<'K,'V> with
    static member ofTuples (tuples:seq<_*_>) = 
        let d = Dictionary()
        d.AddAll tuples
        d

type HashSet<'T> with
    static member ofSeq (s:seq<'T>) = HashSet s
    static member ofSeqCmp (s, cmp) =  HashSet(s, cmp)

let rec private rmdir dir =
    if Directory.Exists dir then
        for f in Directory.EnumerateFiles dir do
            File.Delete f
        for d in Directory.EnumerateDirectories dir do
            rmdir d
        Directory.Delete dir

type Directory with
    /// deletes a directory and its contents recursively, no exception if the directory does not exist
    static member DeleteRec dir = rmdir dir

type Path with
    static member combine path1 path2 = Path.Combine(path1, path2)
    /// gets the absolute path, no trailing slash if it is a directory
    static member absolute path = (Path.GetFullPath path).TrimEnd [|'\\'|]
    static member GetDirectoryNames file =
        seq {
            let path = ref (Path.GetDirectoryName file)
            while false = String.IsNullOrEmpty !path do
                yield !path
                path := Path.GetDirectoryName !path
        }

type File with
    /// file copy, overwrite
    static member copy src dest = File.Copy(src, dest, true)
    /// file copy, throws if already exists
    static member copyNo src dest = File.Copy(src, dest, false)
    /// copies a file to the directory, overwrites
    static member copyTo file dir = File.copy file (Path.combine dir (Path.GetFileName file))
    /// copies a file to the directory, throws if already exists
    static member copyToNo file dir = File.copyNo file (Path.combine dir (Path.GetFileName file))

type Byte with
    static member concat (a:byte[]) (b:byte[]) =
        let c = Array.create (a.Length + b.Length) 0uy
        Buffer.BlockCopy(a, 0, c, 0, a.Length)
        Buffer.BlockCopy(b, 0, c, a.Length, b.Length)
        c