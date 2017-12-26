using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using System;
using System.Text;
using IO = System.IO;
using System.Collections.Generic;

namespace SourceLink.Create
{
    public abstract class GitCreateTask : MSBuildTask
    {
        public string GitDirectory { get; set; }

        public string Url { get; set; }

        public string[] Sources { get; set; }

        [Required]
        public string File { get; set; }

        public string NotInGit { get; set; }

        public string HashMismatch { get; set; }

        public string NoAutoLF { get; set; }

        [Output]
        public string SourceLink { get; set; }

        public string[] EmbeddedFilesIn { get; set; }

        [Output]
        public string[] EmbeddedFiles { get; set; }

        public override bool Execute()
        {
            var url = Url;
            var gitOption = string.IsNullOrEmpty(GitDirectory) ? "" : " -d \"" + GitDirectory + "\"";

            if (string.IsNullOrEmpty(url))
            {
                var originArgs = "sourcelink-git origin" + gitOption;
                var originCmd = Process.RunAndGetOutput("dotnet", originArgs);
                if (originCmd.ExitCode != 0 || originCmd.OutputLines.Count != 1)
                {
                    Log.LogError("unable to find repository origin with: dotnet " + originArgs);
                    return false;
                }
                var origin = originCmd.OutputLines[0];
                url = ConvertUrl(origin);
                if (url == null)
                {
                    Log.LogError("unable to convert origin url: " + origin);
                    return false;
                }
            }

            var compileFile = IO.Path.ChangeExtension(File, ".compile");
            using (var sw = FileUtil.OpenWrite(compileFile))
            {
                if (Sources != null)
                {
                    foreach (var source in Sources)
                        sw.WriteLine(source);
                }
            }

            var sbArgs = new StringBuilder();
            sbArgs.Append("sourcelink-git create" + gitOption);
            sbArgs.Append(" -u \"" + url + "\"");
            sbArgs.Append(" -f \"" + File + "\"");

            if (!string.IsNullOrEmpty(NotInGit))
            {
                sbArgs.Append(" --notingit \"" + NotInGit + "\"");
            }
            if (!string.IsNullOrEmpty(HashMismatch))
            {
                sbArgs.Append(" --hashmismatch \"" + HashMismatch + "\"");
            }
            if ("true".Equals(NoAutoLF, StringComparison.OrdinalIgnoreCase))
            {
                sbArgs.Append(" --noautolf");
            }
            var args = sbArgs.ToString();

            var create = Process.RunAndGetOutput("dotnet", args);
            if (create.ExitCode != 0)
            {
                Log.LogMessage(MessageImportance.High, "dotnet " + args);
                foreach (var line in create.OutputLines)
                    Log.LogMessage(MessageImportance.High, line);
                Log.LogError("exit code " + create.ExitCode + " when running: dotnet " + args);
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal, "dotnet " + args);
                foreach (var line in create.OutputLines)
                    Log.LogMessage(MessageImportance.Normal, line);
            }

            if (Log.HasLoggedErrors)
                return false;

            var embeddedFiles = EmbeddedFilesIn == null ? new List<string>() : new List<string>(EmbeddedFilesIn);
            var embedFile = IO.Path.ChangeExtension(File, ".embed");
            if (IO.File.Exists(embedFile))
            {
                var additionalFiles = IO.File.ReadAllLines(embedFile);
                embeddedFiles.AddRange(additionalFiles);
            }
            EmbeddedFiles = embeddedFiles.ToArray();

            SourceLink = File;
            return true;
        }

        public abstract string ConvertUrl(string origin);
   
    }
}
