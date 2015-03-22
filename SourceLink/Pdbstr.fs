namespace SourceLink

open System
open System.IO

type Pdbstr() =
    static member tryFind() =
        [
        // Chocolatey, `cinst sourcelink` adds pdbstr too, default is C:\ProgramData\chocolatey\bin\pdbstr.exe
        Path.combine (Environment.GetFolderPath Environment.SpecialFolder.CommonApplicationData) @"chocolatey\bin\pdbstr.exe"
        @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe" // 6.3.9600.16384
        @"C:\Program Files\Microsoft Team Foundation Server 12.0\Tools\pdbstr.exe" // 6.3.9600.16384
        @"C:\Program Files (x86)\Windows Kits\8.0\Debuggers\x64\srcsrv\pdbstr.exe" // 6.2.9200.16384
        @"C:\Program Files\Microsoft Team Foundation Server 11.0\Tools\pdbstr.exe" // 6.2.9200.16384
        @"C:\Program Files\Debugging Tools for Windows (x64)\srcsrv\pdbstr.exe"
        ]
        |> List.tryFind File.Exists
