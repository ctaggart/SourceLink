// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Samples.Debugging.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;

//namespace Roslyn.Test.PdbUtilities
namespace SourceLink.SymbolStore
{
    public sealed class PdbReader : IDisposable
    {
        private ISymUnmanagedReader rawReader;
        private ISymbolReader symReader;
        private IntPtr sessionCookie;
        private long moduleCookie;

        public PdbReader(ISymUnmanagedReader rawReader, IntPtr sessionCookie, string fileName)
        {
            this.rawReader = rawReader;
            this.symReader = SymbolBinder.GetReaderFromCOM(rawReader);
            this.sessionCookie = sessionCookie;
            if (String.IsNullOrEmpty(fileName))
                this.moduleCookie = 0;
            else
                this.moduleCookie = SrcSrv.LoadModule(sessionCookie, fileName, this.ISymUnmanagedSourceServerModule);
        }

        public PdbReader(Stream pdb, IntPtr sessionCookie, string fileName) :
            this(CreateRawReader(pdb), sessionCookie, fileName)
        {
        }

        public PdbReader(Stream pdb) :
            this(pdb, IntPtr.Zero, null)
        {
        }

        public static ISymUnmanagedReader CreateRawReader(Stream pdb)
        {
            Guid CLSID_CorSymReaderSxS = new Guid("0A3976C5-4529-4ef8-B0B0-42EED37082CD");
            var rawReader = (ISymUnmanagedReader)Activator.CreateInstance(Type.GetTypeFromCLSID(CLSID_CorSymReaderSxS));
            rawReader.Initialize(DummyMetadataImport.Instance, null, null, new ComStreamWrapper(pdb));
            return rawReader;
        }

        public void Dispose()
        {
            if (this.symReader != null)
            {
                ((IDisposable)this.symReader).Dispose();
                this.symReader = null;
                this.rawReader = null;
            }
        }

        public ISymbolReader ISymbolReader
        {
            get
            {
                if (symReader == null)
                    throw new ObjectDisposedException("SymReader");
                return symReader;
            }
        }

        public ISymUnmanagedReader ISymUnmanagedReader
        {
            get
            {
                if (rawReader == null)
                    throw new ObjectDisposedException("SymReader");
                return rawReader;
            }
        }

        public ISymUnmanagedReader2 ISymUnmanagedReader2
        {
            get { return (ISymUnmanagedReader2)rawReader; }
        }

        public ISymUnmanagedSourceServerModule ISymUnmanagedSourceServerModule
        {
            get { return (ISymUnmanagedSourceServerModule)rawReader; }
        }

        public bool IsSourceIndexed
        {
            get { return moduleCookie != 0L; }
        }

        public string GetDownloadUrl(string sourceFilePath)
        {
            if (!IsSourceIndexed) return null;
            return SrcSrv.GetFileUrl(sessionCookie, moduleCookie, sourceFilePath);
        }

        // derived from Roslyn.Test.PdbUtilities.PdbToXmlConverter
        public ISymUnmanagedMethod[] GetMethodsInDocument(ISymUnmanagedDocument symDocument)
        {
            var symReader = this.ISymUnmanagedReader2;
            int count;
            symReader.GetMethodsInDocument(symDocument, 0, out count, null);
            var methods = new ISymUnmanagedMethod[count];
            symReader.GetMethodsInDocument(symDocument, count, out count, methods);
            return methods;
        }
    }

    public static class PdbReaderExt
    {
        public static ISymUnmanagedDocument[] GetDocuments(this ISymUnmanagedReader reader)
        {
            int count;
            reader.GetDocuments(0, out count, null);
            var docs = new ISymUnmanagedDocument[count];
            reader.GetDocuments(count, out count, docs);
            return docs;
        }

        public static SequencePoint[] GetSequencePoints(this ISymbolMethod method)
        {
            int count = method.SequencePointCount;
            int[] offsets = new int[count];
            ISymbolDocument[] docs = new ISymbolDocument[count];
            int[] startColumn = new int[count];
            int[] endColumn = new int[count];
            int[] startRow = new int[count];
            int[] endRow = new int[count];
            method.GetSequencePoints(offsets, docs, startRow, startColumn, endRow, endColumn);
            var points = new SequencePoint[count];
            for (int i = 0; i < count; i++)
                points[i] = new SequencePoint(offsets[i], docs[i], startRow[i], startColumn[i], endRow[i], endColumn[i]);
            return points;
        }
    }
}
