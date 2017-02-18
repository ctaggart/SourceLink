using System;
using LibGit2Sharp;
using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

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

        public override bool Execute()
        {
            //Log.LogError
            //Log.LogMessage
            //Log.LogWarning
            Log.LogWarning("File is " + File);
            //File = "other file"

            using (var sw = System.IO.File.CreateText(File))
            {
                sw.WriteLine("{\"documents\": { \"C:\\\\Users\\\\camer\\\\cs\\\\sourcelink-test\\\\*\" : \"https://raw.githubusercontent.com/ctaggart/sourcelink-test/b5012a98bed12f6704cb942e92ba34ccdbd920d8/*\" }}");
            }

            SourceLink = File;
            return true;
        }

    }
}
