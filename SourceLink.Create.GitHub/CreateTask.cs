using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using System.Diagnostics;

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

            // TODO get the url from the repo if not set

            bool captureOutput = true; // TODO can we get the verbosity level?
            var exit = Process.Run("dotnet", "sourcelink-git repo",
                outputHandler: captureOutput ? LogMessageHander(MessageImportance.Normal) : null,
                errorHandler: captureOutput ? LogMessageHander(MessageImportance.Normal) : null
            );
            Log.LogMessage(MessageImportance.High, "ExitCode: " + exit);

            // TODO tell dotnet sourcelink-git to create

            using (var sw = System.IO.File.CreateText(File))
            {
                sw.WriteLine("{\"documents\": { \"C:\\\\Users\\\\camer\\\\cs\\\\sourcelink-test\\\\*\" : \"https://raw.githubusercontent.com/ctaggart/sourcelink-test/b5012a98bed12f6704cb942e92ba34ccdbd920d8/*\" }}");
            }

            SourceLink = File;
            return true;
        }

    }
}
