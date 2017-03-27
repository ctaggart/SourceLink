using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace SourceLink
{
    public class DebugReaderProvider : IDisposable
    {
        private Stream stream;
        private MetadataReaderProvider provider;

        public DebugReaderProvider(string path)
        {
            stream = File.OpenRead(path);
            if (path.EndsWith(".dll"))
            {
                var reader = new PEReader(stream);
                if (reader.HasMetadata)
                {
                    // https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/tests/PortableExecutable/PEReaderTests.cs#L392
                    var debugDirectoryEntries = reader.ReadDebugDirectory();
                    if (debugDirectoryEntries.Length < 3) return;
                    provider = reader.ReadEmbeddedPortablePdbDebugDirectoryData(debugDirectoryEntries[2]);
                }
            }
            else
            {
                provider = MetadataReaderProvider.FromPortablePdbStream(stream);
            }
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
