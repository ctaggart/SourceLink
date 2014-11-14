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
    public sealed class PdbReader : IDisposable, ISymbolReader
    {
        private ISymUnmanagedReader rawReader;
        private ISymbolReader symReader;
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
                this.moduleCookie = SrcSrv.LoadModule(sessionCookie, fileName, this.SymUnmanagedSourceServerModule);
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

        public ISymbolReader SymbolReader
        {
            get
            {
                if (symReader == null)
                {
                    throw new ObjectDisposedException("SymReader");
                }

                return symReader;
            }
        }

        public ISymUnmanagedReader SymUnmanagedReader
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

        public ISymUnmanagedSourceServerModule SymUnmanagedSourceServerModule
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

        //  explicit ISymbolReader

        ISymbolDocument ISymbolReader.GetDocument(string url, Guid language, Guid languageVendor, Guid documentType)
        {
            return this.symReader.GetDocument(url, language, languageVendor, documentType);
        }

        ISymbolDocument[] ISymbolReader.GetDocuments()
        {
            return this.symReader.GetDocuments();
        }

        ISymbolVariable[] ISymbolReader.GetGlobalVariables()
        {
            return this.symReader.GetGlobalVariables();
        }

        ISymbolMethod ISymbolReader.GetMethod(SymbolToken method, int version)
        {
            return this.symReader.GetMethod(method, version);
        }

        ISymbolMethod ISymbolReader.GetMethod(SymbolToken method)
        {
            return this.symReader.GetMethod(method);
        }

        ISymbolMethod ISymbolReader.GetMethodFromDocumentPosition(ISymbolDocument document, int line, int column)
        {
            return this.symReader.GetMethodFromDocumentPosition(document, line, column);
        }

        ISymbolNamespace[] ISymbolReader.GetNamespaces()
        {
            return this.symReader.GetNamespaces();
        }

        byte[] ISymbolReader.GetSymAttribute(SymbolToken parent, string name)
        {
           return this.symReader.GetSymAttribute(parent, name);
        }

        ISymbolVariable[] ISymbolReader.GetVariables(SymbolToken parent)
        {
            return this.symReader.GetVariables(parent);
        }

        SymbolToken ISymbolReader.UserEntryPoint
        {
            get { return this.symReader.UserEntryPoint; }
        }

        // implicit ISymbolReader

        public ISymbolDocument GetDocument(string url, Guid language, Guid languageVendor, Guid documentType)
        {
            return this.symReader.GetDocument(url, language, languageVendor, documentType);
        }

        public ISymbolDocument[] GetDocuments()
        {
            return this.symReader.GetDocuments();
        }

        public ISymbolVariable[] GetGlobalVariables()
        {
            return this.symReader.GetGlobalVariables();
        }

        public ISymbolMethod GetMethod(SymbolToken method, int version)
        {
            return this.symReader.GetMethod(method, version);
        }

        public ISymbolMethod GetMethod(SymbolToken method)
        {
            return this.symReader.GetMethod(method);
        }

        public ISymbolMethod GetMethodFromDocumentPosition(ISymbolDocument document, int line, int column)
        {
            return this.symReader.GetMethodFromDocumentPosition(document, line, column);
        }

        public ISymbolNamespace[] GetNamespaces()
        {
            return this.symReader.GetNamespaces();
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

        // convenience getters

        public ISymbolDocument[] Documents
        {
            get { return this.symReader.GetDocuments(); }
        }

        public ISymbolVariable[] GlobalVariables
        {
            get { return this.symReader.GetGlobalVariables(); }
        }

        public ISymbolNamespace[] Namespaces
        {
            get { return this.symReader.GetNamespaces(); }
        }

    }
}
