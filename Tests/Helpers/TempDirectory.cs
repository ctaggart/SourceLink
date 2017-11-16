using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Tests.Helpers
{
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class TempDirectory : IDisposable
    {
        public TempDirectory() : this(System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName()))
        {
        }

        public TempDirectory(string path)
        {
            Directory.CreateDirectory(path);
            this.path = path;
        }

        private string path;
        public string Path => path;

        public static implicit operator string(TempDirectory tempFile) => tempFile.path;

        public override string ToString() => path;

        public void Dispose()
        {
            var path = Interlocked.Exchange(ref this.path, null);
            if (path != null) Directory.Delete(path, recursive: true);
        }
    }
}
