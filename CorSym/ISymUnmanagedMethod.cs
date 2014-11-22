// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

//namespace Roslyn.Utilities.Pdb
namespace SourceLink.SymbolStore.CorSym
{
    // COM interface method order is important. These methods must match:
    // https://github.com/ctaggart/SourceLink/blob/8e4a3a5d2c4c9b9179b23d2be560ecf8822f9d22/CorSym/corsym.h#L1176-L1242

    [ComImport, Guid("B62B923C-B500-3158-A543-24F307A8B7E1"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false)]
    public interface ISymUnmanagedMethod
    {
        void GetToken(out int token);
        void GetSequencePointCount(out int count);
        void GetRootScope([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope scope);
        void GetScopeFromOffset(int offset, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope scope);
        void GetOffset(ISymUnmanagedDocument document, int line, int column, out int offset);
        void GetRanges(ISymUnmanagedDocument document, int line, int column, int cRanges, out int pcRanges, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] ranges);
        void GetParameters(int cParams, out int pcParams, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] parms);
        void GetNamespace([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedNamespace ns);
        void GetSourceStartEnd(ISymUnmanagedDocument[] docs, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] lines, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] columns, out Boolean retVal);
        void GetSequencePoints(
            int cPoints,
            out int pcPoints,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] offsets,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] lines,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] columns,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endLines,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endColumns);
    }
}
