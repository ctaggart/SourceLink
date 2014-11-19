// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Text;

//namespace Roslyn.Utilities.Pdb
namespace SourceLink.SymbolStore.CorSym
{
    // COM interface method order is important. These methods must match:
    // https://github.com/ctaggart/SourceLink/blob/8e4a3a5d2c4c9b9179b23d2be560ecf8822f9d22/CorSym/corsym.h#L895-L944

    [   ComImport, Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false) ]
    public interface ISymUnmanagedDocument
    {
        void GetURL(int cchUrl, out int pcchUrl, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder szUrl);
        void GetDocumentType(ref Guid pRetVal);
        void GetLanguage(ref Guid pRetVal);
        void GetLanguageVendor(ref Guid pRetVal);
        void GetCheckSumAlgorithmId(ref Guid pRetVal);
        void GetCheckSum(int cData, out int pcData, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] data);
        void FindClosestLine(int line, out int pRetVal);
        void HasEmbeddedSource(out bool pRetVal);
        void GetSourceLength(out int pRetVal);
        void GetSourceRange(int startLine, int startColumn, int endLine, int endColumn, int cSourceBytes, out int pcSourceBytes, 
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] source);
    }
}
