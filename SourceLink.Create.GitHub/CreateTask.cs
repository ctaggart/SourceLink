using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using System.Diagnostics;
using System;
using System.Text;

namespace SourceLink.Create.GitHub
{
    public class CreateTask : MSBuildTask
    {
        public string Url { get; set; }

        public string Repo { get; set; }

        [Required]
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
            var repo = Repo;
            if (String.IsNullOrEmpty(repo))
            {
                var originCmd = Process.RunAndGetOutput("dotnet", "sourcelink-git origin");
                if (originCmd.ExitCode != 0 || originCmd.OutputLines.Count != 1)
                {
                    Log.LogMessage(MessageImportance.High, "unable to get repository origin");
                    return false;
                }
                repo = originCmd.OutputLines[0];
            }

            var args = new StringBuilder();
            args.Append("sourcelink-git create");

            var exit = Process.Run("dotnet", args.ToString(),
                outputHandler: LogMessageHander(MessageImportance.Normal),
                errorHandler: LogMessageHander(MessageImportance.Normal)
            );

            SourceLink = File;
            return exit == 0;
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
            return "https://raw.githubusercontent.com" + uri.LocalPath + "/{0}/*";
        }

    }
}
