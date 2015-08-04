[<AutoOpen>]
module SourceLink.Version

open System.Reflection

let version =
    let assembly = Assembly.GetExecutingAssembly()
    let iv = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() |> Option.ofObj |> Option.map (fun at -> at.InformationalVersion)
    match iv with
    | None -> ""
    | Some iv -> 
        let ss = iv.Split [|'\"'|]
        if ss.Length > 3 then ss.[3] else ""