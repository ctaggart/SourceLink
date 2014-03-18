#r "../Dia/bin/Debug/Microsoft.Dia.dll"
#r "../Dia/bin/Debug/SourceLink.Dia.dll"
#r "../SourceLink/bin/Debug/SourceLink.dll"

open System
open System.IO
open Microsoft.Dia
open SourceLink.Dia
open SourceLink

let printGuidAge file =
    let sn = IDiaSession.Open file
    let gs = sn.globalScope
    printfn "%A %d" gs.guid gs.age

let printTables file =
    let sn = IDiaSession.Open file
    for t in sn.SeqTables() do
        printfn "%s %d" t.name t.count

let printCompilands file =
    let sn = IDiaSession.Open file
    let cs = sn.findChildren(sn.globalScope, SymTagEnum.SymTagCompiland, null, 0u)
    for c in cs.Seq() do
        printfn "%d %s" c.symIndexId c.name

let printSourceFiles file =
    let sn = IDiaSession.Open file
    let sfs = sn.Tables.SourceFiles
    printfn "# of source files %d" sfs.count
    for sf in sfs.Seq() do
        printfn "%d %s" sf.uniqueId sf.fileName

let printSourceFileCompilands file =
    let sn = IDiaSession.Open file
    let sfs = sn.Tables.SourceFiles
    for sf in sfs.Seq() do
        printfn "%s" sf.fileName
        for sym in sf.compilands.Seq() do
            printfn "  %s" sym.name

let file = Path.combine __SOURCE_DIRECTORY__ @"..\packages\FSharp.Data.2.0.3\lib\net40\FSharp.Data.DesignTime.pdb"
printGuidAge file
printTables file
printCompilands file
printSourceFiles file
printSourceFileCompilands file