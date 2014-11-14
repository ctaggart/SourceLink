using System;
using System.IO;

namespace SourceLink.SymbolStore
{
    public class SymbolCache
    {
        string symbolCacheDir;
        IntPtr sessionCookie;

        public SymbolCache(string symbolCacheDir)
        {
            this.symbolCacheDir = symbolCacheDir;
            this.sessionCookie = new IntPtr(new Random().Next());
            SrcSrv.Init(sessionCookie, symbolCacheDir);
        }

        public PdbReader ReadPdb(string filePath, Stream stream)
        {
            return new PdbReader(stream, sessionCookie, filePath);
        }

        public string DownloadFile(string downloadUrl)
        {
            return SymSrv.DownloadFile(downloadUrl, this.symbolCacheDir);
        }
    }
}
