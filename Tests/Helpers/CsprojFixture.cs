using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests.Helpers
{
    public sealed class CsprojFixture : IDisposable
    {
        private readonly TempDirectory tempDir;
        private const string CsprojName = "test.csproj";
        private const string PackagesCache = "packages";

        public CsprojFixture(string[] csprojLines)
        {
            tempDir = new TempDirectory();

            File.WriteAllLines(Path.Combine(tempDir, CsprojName), csprojLines);
        }

        public void Dispose()
        {
            tempDir.Dispose();
        }

        public void AddFile(string path, string[] lines)
        {
            File.WriteAllLines(Path.Combine(tempDir, path), lines);
        }

        public void DotnetRestore(string packageSource)
        {
            RunProcess(tempDir, "dotnet", $"restore --no-cache --packages \"{PackagesCache}\" --source \"{Path.GetFullPath(packageSource)}\"");
        }

        public void DotnetMSBuild()
        {
            RunProcess(tempDir, "dotnet", "msbuild");
        }

        private static void RunProcess(string workingDirectory, string fileName, string arguments)
        {
            var result = ProcessUtils.Run(new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = "dotnet",
                Arguments = arguments
            });

            var hasErrorOutput = result.StandardStreamData.Any(_ => _.IsError);
            if (hasErrorOutput || result.ExitCode != 0)
            {
                var message = new StringBuilder(fileName).Append(' ').Append(arguments).Append(" exited with code ").Append(result.ExitCode);

                if (hasErrorOutput) message.Append(" and wrote to stderr");

                if (result.StandardStreamData.Length == 0)
                {
                    message.Append(" and no output.");
                }
                else
                {
                    message.Append(':');
                    foreach (var data in result.StandardStreamData)
                        message.AppendLine().Append(data);
                }

                throw new Exception(message.ToString());
            }
        }

        public string[] GetFiles(string relativePath, SearchOption searchOption)
        {
            return GetFiles(relativePath, null, searchOption);
        }

        public string[] GetFiles(string relativePath, string searchPattern, SearchOption searchOption)
        {
            if (relativePath != null && Path.IsPathRooted(relativePath))
                throw new ArgumentException("Path must be relative.", nameof(relativePath));

            var files = Directory.GetFiles(Path.Combine(tempDir, relativePath), searchPattern ?? "*", searchOption);

            var tempDirPathLength = tempDir.Path.Length + 1;

            for (var i = 0; i < files.Length; i++)
                files[i] = files[i].Substring(tempDirPathLength);

            return files;
        }
    }
}
