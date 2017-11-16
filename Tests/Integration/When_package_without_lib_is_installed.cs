using System;
using System.IO;
using Tests.Helpers;
using Xunit;

namespace Tests.Integration
{
    public static class When_package_without_lib_is_installed
    {
        [Theory]
        [InlineData("SourceLink.Create.BitBucket")]
        [InlineData("SourceLink.Create.BitBucketServer")]
        [InlineData("SourceLink.Create.CommandLine")]
        [InlineData("SourceLink.Create.GitHub")]
        [InlineData("SourceLink.Create.GitLab")]
        [InlineData("SourceLink.Embed.AllSourceFiles")]
        [InlineData("SourceLink.Embed.PaketFiles")]
        [InlineData("SourceLink.Test")]
        public static void Should_not_reference_additional_libraries(string packageName)
        {
            var packageSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration");

            var packageFile = Assert.Single(Directory.GetFiles(packageSource, $"{packageName}.*.nupkg"));
            var packageVersion = Path.GetFileNameWithoutExtension(packageFile).Substring(packageName.Length + 1);

            const string targetFramework = "net462";
            using (var fixture = new CsprojFixture(new[]
            {
                "<Project Sdk=\"Microsoft.Net.Sdk\">",
                "  <PropertyGroup>",
               $"    <TargetFramework>{targetFramework}</TargetFramework>",
                "  </PropertyGroup>",
                "  <ItemGroup>",
               $"    <PackageReference Include=\"{packageName}\" Version=\"{packageVersion}\" PrivateAssets=\"all\" />",
                "  </ItemGroup>",
                "</Project>"
            }))
            {
                fixture.AddFile("test.cs", Array.Empty<string>());

                fixture.DotnetRestore(packageSource);
                fixture.DotnetMSBuild();

                Assert.Equal(
                    new[]
                    {
                        $@"bin\Debug\{targetFramework}\test.dll",
                        $@"bin\Debug\{targetFramework}\test.pdb"
                    },
                    fixture.GetFiles($@"bin\Debug\{targetFramework}", SearchOption.AllDirectories));
            }
        }
    }
}
