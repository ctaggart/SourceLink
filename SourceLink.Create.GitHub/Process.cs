using System.Diagnostics;

// TODO move to SourceLink.Core assembly & nupkg
namespace SourceLink
{
    public static class Process
    {
        public static int Run(string filename, string arguments = "", string workingDirectory = "", DataReceivedEventHandler outputHandler = null, DataReceivedEventHandler errorHandler = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                WorkingDirectory = "",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = outputHandler != null,
                RedirectStandardError = errorHandler != null,
            };
            using (var p = new System.Diagnostics.Process())
            {
                p.StartInfo = psi;
                p.OutputDataReceived += outputHandler;
                p.ErrorDataReceived += errorHandler;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                return p.ExitCode;
            }
        }
    }
}