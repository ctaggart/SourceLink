namespace SourceLink

open System
open System.IO

type Pdbstr() =
    static member tryFind() =
        [
        // Chocolatey, `choco install sourcelink` adds pdbstr too, default is C:\ProgramData\chocolatey\bin\pdbstr.exe
        Path.combine (Environment.GetFolderPath Environment.SpecialFolder.CommonApplicationData) @"chocolatey\bin\pdbstr.exe" // x86 6.3.9600.17298, Windows 8.1 Update 1
        @"C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\srcsrv\pdbstr.exe" // 10.0.10240.16384
        @"C:\Program Files (x86)\Windows Kits\10\Debuggers\x86\srcsrv\pdbstr.exe" // 10.0.10240.16384
        @"C:\Program Files (x86)\Windows Kits\8.1\Debuggers\x64\srcsrv\pdbstr.exe" // 6.3.9600.16384, Windows 8.1
        @"C:\Program Files\Microsoft Team Foundation Server 12.0\Tools\pdbstr.exe" // 6.3.9600.16384
        @"C:\Program Files (x86)\Windows Kits\8.0\Debuggers\x64\srcsrv\pdbstr.exe" // 6.2.9200.16384
        @"C:\Program Files\Microsoft Team Foundation Server 11.0\Tools\pdbstr.exe" // 6.2.9200.16384
        @"C:\Program Files\Debugging Tools for Windows (x64)\srcsrv\pdbstr.exe"
        ]
        |> List.tryFind File.Exists
