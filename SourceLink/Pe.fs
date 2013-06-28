module SourceLink.Pe

open Mono.Cecil
open System
open System.Globalization

let toString bytes = Text.Encoding.UTF8.GetString bytes // UTF8?

let defaultGuid = Guid() // Guid.ParseExact("00000000000000000000000000000000","N") // same
let unixEpoch = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

// for decoding hex value from running dumpbin /headers file.dll
let toDateTime (timestamp:string) =
    if timestamp.Length <> 8 then
        failwith "timestamp not 4 bytes in hex: %s" timestamp
    let bytes = Hex.decode timestamp
    if BitConverter.IsLittleEndian then
        Array.Reverse bytes
    let seconds = BitConverter.ToInt32(bytes, 0)
    unixEpoch.AddSeconds (double seconds)

let formatDate (dt:DateTime) =
    let s = dt.ToString DateTimeFormatInfo.InvariantInfo.SortableDateTimePattern
    if dt.Kind = DateTimeKind.Utc then s+"Z" else s

type Guid with
    member x.ToStringN with get() = x.ToString("N").ToUpperInvariant()

type PeInfo() =
    member val File = String.Empty with set, get
    member val Guid = defaultGuid with set, get
    member val Age = 0u with set, get
    member x.Id with get() = x.Guid.ToStringN + x.Age.ToString()
    member val DateTime = unixEpoch with set, get
    member val Pdb = String.Empty with set, get

let readPeInfo (file:string) =
    let m = ModuleDefinition.ReadModule file
    let header, bytes = m.GetDebugHeader()
    let pi = PeInfo()
    pi.File <- file
    pi.DateTime <- unixEpoch.AddSeconds (double header.TimeDateStamp)
    let rsds = toString bytes.[0..3]
    if rsds = "RSDS" then
        pi.Guid <- Guid bytes.[4..19]
        pi.Age <- BitConverter.ToUInt32(bytes, 20)
        pi.Pdb <- (toString bytes.[24..]).TrimEnd [|(char)0uy|]
    pi