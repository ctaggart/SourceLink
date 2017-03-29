using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Linq;

namespace SourceLink
{
    public class DebugReaderProvider : IDisposable
    {
        public string Path {get; private set; }
        private Stream stream;
        private MetadataReaderProvider provider;

        public DebugReaderProvider(string path, Stream stream)
        {
            Path = path;
            this.stream = stream;
            if (path.EndsWith(".dll"))
            {
                var reader = new PEReader(stream);
                if (reader.HasMetadata)
                {
                    // https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/tests/PortableExecutable/PEReaderTests.cs
                    var debugDirectoryEntries = reader.ReadDebugDirectory();
                    var embeddedPdb = debugDirectoryEntries.Where(dde => dde.Type == DebugDirectoryEntryType.EmbeddedPortablePdb).FirstOrDefault();
                    if (!embeddedPdb.Equals(default(DebugDirectoryEntry)))
                    {
                        provider = reader.ReadEmbeddedPortablePdbDebugDirectoryData(embeddedPdb);
                    }
                }
            }
            else
            {
                provider = MetadataReaderProvider.FromPortablePdbStream(stream);
            }
        }

        public DebugReaderProvider(string path)
            : this(path, File.OpenRead(path))
        {
        }

        public MetadataReader GetMetaDataReader()
        {
            return provider?.GetMetadataReader();
        }

        // Basic Dispose Pattern https://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx#Anchor_0
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (provider != null) provider.Dispose();
                if (stream != null) stream.Dispose();
            }
        }
    }


}
