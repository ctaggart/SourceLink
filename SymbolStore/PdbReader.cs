// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
//using Microsoft.Samples.Debugging.SymbolStore;
//using Roslyn.Utilities;
//using Roslyn.Utilities.Pdb;
using System.Diagnostics.SymbolStore;

//namespace Roslyn.Test.PdbUtilities
namespace SourceLink.SymbolStore
{
    public sealed class PdbReader : IDisposable
    {
        private ISymUnmanagedReader rawReader;
        private SymReader symReader;
        private IntPtr sessionCookie;
        private long moduleCookie;

        public PdbReader(ISymUnmanagedReader rawReader, IntPtr sessionCookie, string fileName)
        {
            this.rawReader = rawReader;
            this.symReader = new SymReader(rawReader);
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

        public static object CreateUnmanagedReader(Stream pdb)
        {
            return CreateRawReader(pdb);
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

        public ISymUnmanagedReader ISymUnmanagedReader
        {
            get
            {
                if (rawReader == null)
                {
                    throw new ObjectDisposedException("SymReader");
                }

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

        public SymDocument[] Documents
        {
            get { return this.symReader.GetDocuments(); }
        }

        // ISymbolReader methods, removed some that throw NotImplementedException
        // some of these may too, TODO test them

        public ISymbolDocument GetDocument(string url, Guid language, Guid languageVendor, Guid documentType)
        {
            return this.symReader.GetDocument(url, language, languageVendor, documentType);
        }
        public SymMethod GetMethod(SymbolToken method, int version)
        {
            return this.symReader.GetMethod(method, version);
        }

        public SymMethod GetMethod(SymbolToken method)
        {
            return this.symReader.GetMethod(method);
        }

        public SymMethod GetMethodFromDocumentPosition(ISymbolDocument document, int line, int column)
        {
            return this.symReader.GetMethodFromDocumentPosition(document, line, column);
        }

        public byte[] GetSymAttribute(SymbolToken parent, string name)
        {
            return this.symReader.GetSymAttribute(parent, name);
        }

        public ISymbolVariable[] GetVariables(SymbolToken parent)
        {
            return this.symReader.GetVariables(parent);
        }

        public SymbolToken UserEntryPoint
        {
            get { return this.symReader.UserEntryPoint; }
        }

    }
}
