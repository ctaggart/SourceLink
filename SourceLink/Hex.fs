// http://fssnip.net/25 and slightly modified
module SourceLink.Hex

open System

let toHexDigit n =
    if n < 10 then char (n + 0x30) else char (n + 0x37)
    
let fromHexDigit c =
    if c >= '0' && c <= '9' then int c - int '0'
    elif c >= 'A' && c <= 'F' then (int c - int 'A') + 10
    elif c >= 'a' && c <= 'f' then (int c - int 'a') + 10
    else raise <| new ArgumentException()
        
let encode (buf:byte array) =
    let hex = Array.zeroCreate (buf.Length * 2)
    let mutable n = 0
    for i = 0 to buf.Length - 1 do
        hex.[n] <- toHexDigit ((int buf.[i] &&& 0xF0) >>> 4)
        n <- n + 1
        hex.[n] <- toHexDigit (int buf.[i] &&& 0xF)
        n <- n + 1
    String hex
            
let decode (s:string) =
    match s with
    | null -> nullArg "s"
    | _ when s.Length = 0 -> Array.empty
    | _ ->
        let mutable len = s.Length
        let mutable i = 0
        if len >= 2 && s.[0] = '0' && (s.[1] = 'x' || s.[1] = 'X') then do
            len <- len - 2
            i <- i + 2
        if len % 2 <> 0 then invalidArg "s" "Invalid hex format"
        else
            let buf = Array.zeroCreate (len / 2)
            let mutable n = 0
            while i < s.Length do
                buf.[n] <- byte (((fromHexDigit s.[i]) <<< 4) ||| (fromHexDigit s.[i + 1]))
                i <- i + 2
                n <- n + 1
            buf
