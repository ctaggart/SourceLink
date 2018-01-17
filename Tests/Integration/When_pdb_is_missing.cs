using System;
using System.IO;
using Xunit;

namespace Tests.Integration
{
    public class When_pdb_is_missing
    {
        // If the pdb file is not found and it is not embedded, it should fail.
        [Fact]
        public void Dll_should_fail()
        {
            var dll = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget/packages/google.protobuf/3.5.1/lib/netstandard1.0/Google.Protobuf.dll");
            var exit = SourceLink.Program.Main(new[] { "test", dll });
            Assert.NotEqual(0, exit);
        }

        [Fact]
        public void Nupkg_should_fail()
        {
            var dll = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget/packages/google.protobuf/3.5.1/google.protobuf.3.5.1.nupkg");
            var exit = SourceLink.Program.Main(new[] { "test", dll });
            Assert.NotEqual(0, exit);
        }
    }
}
