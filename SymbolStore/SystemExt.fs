[<AutoOpen>]
module SourceLink.SymbolStore.SystemExt

type Option<'A> with
    static member ofNull (t:'T when 'T : equality) =
        if t = null then None else Some t