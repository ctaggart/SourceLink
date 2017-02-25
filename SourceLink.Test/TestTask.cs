using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using System.Diagnostics;
using System.Text;

namespace SourceLink.Test
{
    public class TestTask : MSBuildTask
    {
        [Required]
        public string Pdb { get; set; }

        public override bool Execute()
        {
            var sbArgs = new StringBuilder();
            sbArgs.Append("sourcelink test");
            sbArgs.Append(" \"" + Pdb + "\"");
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

            return !Log.HasLoggedErrors;
        }

    }
}
