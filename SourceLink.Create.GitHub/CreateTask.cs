using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using System.Diagnostics;
using System;
using System.Text;

namespace SourceLink.Create.GitHub
{
    public class CreateTask : MSBuildTask
    {
        public string GitDirectory { get; set; }

        public string Url { get; set; }

        public string[] Sources { get; set; }

        [Required]
        public string File { get; set; }

        [Output]
        public string SourceLink { get; set; }

        DataReceivedEventHandler LogMessageHander(MessageImportance importance)
        {
            return (s, e) => {
                if (e.Data != null) // end
                {
                    Log.LogMessage(importance, e.Data);
                }
            };
        }

        public override bool Execute()
        {
            var url = Url;
            var gitOption = String.IsNullOrEmpty(GitDirectory) ? "" : " -d \"" + GitDirectory + "\"";

            if (String.IsNullOrEmpty(url))
            {
                var originCmd = Process.RunAndGetOutput("dotnet", "sourcelink-git origin" + gitOption);
                if (originCmd.ExitCode != 0 || originCmd.OutputLines.Count != 1)
                {
                    Log.LogMessage(MessageImportance.High, "unable to get repository origin");
                    return false;
                }
                var origin = originCmd.OutputLines[0];
                url = GetRepoUrl(origin);
            }

            var sbArgs = new StringBuilder();
            sbArgs.Append("sourcelink-git create" + gitOption);
            sbArgs.Append(" -u " + url);
            sbArgs.Append(" -f \"" + File + "\"");
            if (Sources != null) {
                foreach (var source in Sources)
                    sbArgs.Append(" -s \"" + source + "\"");
            }
            var args = sbArgs.ToString();

            var create = Process.RunAndGetOutput("dotnet", args);
            if(create.ExitCode != 0)
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

            SourceLink = File;
            return !Log.HasLoggedErrors;
        }

        public static string GetRepoUrl(string origin)
        {
            if (origin.StartsWith("git@"))
            {
                origin = origin.Replace(':', '/');
                origin = origin.Replace("git@", "https://");
            }
            origin = origin.Replace(".git", "");
            var uri = new Uri(origin);
            return "https://raw.githubusercontent.com" + uri.LocalPath + "/{commit}/*";
        }

    }
}
