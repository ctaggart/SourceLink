namespace SourceLink

open System.IO

type Pdbstr() =
    static member tryFind() =
        [
        @"C:\Program Files\Microsoft Team Foundation Server 12.0\Tools\pdbstr.exe" // 6.3.9600.16384
        @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe" // 6.3.9200.16384
        @"C:\Program Files (x86)\Windows Kits\8.0\Debuggers\x64\srcsrv\pdbstr.exe" // 6.2.9200.16384
        ]
        |> List.tryFind File.Exists
