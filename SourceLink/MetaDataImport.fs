namespace SourceLink

open System
open System.Runtime.InteropServices

// "C:\Program Files (x86)\Windows Kits\8.1\Include\um\cor.h"
module Cor =
    [<ComImport; Interface; Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"); InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
    type IMetaDataDispenser = 
        abstract DefineScope : unit -> unit // need this here to fill the first vtable slot
        abstract OpenScope : [<In; MarshalAs(UnmanagedType.LPWStr)>] szScope : string * [<In>] dwOpenFlags:Int32 * [<In>] riid : System.Guid byref * [<Out; MarshalAs(UnmanagedType.IUnknown)>] punk:Object byref -> unit

//    [<ComImport; Guid("e5cb7a31-7512-11d2-89ce-0080c792e5d8")>]
//    type CorMetaDataDispenser() =
//        do ()
//        interface IMetaDataDispenser with
//            override x.DefineScope() = ()
//            override x.OpenScope(szScope, dwOpenFlags, riid, punk) = ()

    [<ComImport; Interface; Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"); InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
    type IMetaDataImport =
        abstract Placeholder : unit -> unit

// "C:\Program Files (x86)\Windows Kits\8.1\Include\um\corsym.h"
module CorSym =
    open Cor

    [<ComImport; Interface; Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5"); InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
    type ISymUnmanagedReader =
        abstract Placeholder : unit -> unit

    [<ComImport; Interface; Guid("ACCEE350-89AF-4ccb-8B40-1C2C4C6F9434"); InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>]
    type ISymUnmanagedBinder2 =
        abstract GetReaderForFile : importer:IMetaDataImport * [<MarshalAs(UnmanagedType.LPWStr)>] filename:string * [<MarshalAs(UnmanagedType.LPWStr)>] searchPath:string * [<Out>] reader:ISymUnmanagedReader -> int
//        abstract GetReaderForFile2 : importer:IMetaDataImport * [<MarshalAs(UnmanagedType.LPWStr)>] filename:string * [<MarshalAs(UnmanagedType.LPWStr)>] searchPath:string * searchPolicy:int * [<Out>] reader:ISymUnmanagedReader -> int

//    [<ComImport; Guid("0A29FF9E-7F9C-4437-8B11-F424491E3931")>]
//    type CorSymBinder =
//        abstract Placeholder : unit -> unit