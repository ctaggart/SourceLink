using System;
using System.Collections.Generic;
using System.Diagnostics;

// TODO move to SourceLink.Core assembly & nupkg
namespace SourceLink
{
    public static class Process
    {
        // search -532462766 or 0xE0434352
        public static readonly int UnhandledExceptionExitCode = -532462766;

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
            try {
                using (var p = new System.Diagnostics.Process())
                {
                    p.StartInfo = psi;
                    if (outputHandler != null)
                        p.OutputDataReceived += outputHandler;
                    if (errorHandler != null)
                        p.ErrorDataReceived += errorHandler;
                    p.Start();
                    if (outputHandler != null)
                        p.BeginOutputReadLine();
                    if (errorHandler != null)
                        p.BeginErrorReadLine();
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch (Exception e)
            {
                return e.HResult;
            }
        }

        // inspired by:
        // https://github.com/dotnet/roslyn/blob/master/src/Tools/Source/RunTests/ProcessRunner.cs
        // https://github.com/dotnet/roslyn/blob/master/src/Test/Utilities/Portable/FX/ProcessUtilities.cs

        public sealed class ProcessOutput
        {
            private readonly int exitCode;
            private readonly IList<string> outputLines;
            private readonly IList<string> errorLines;

            public ProcessOutput(int exitCode, IList<string> outputLines, IList<string> errorLines)
            {
                this.exitCode = exitCode;
                this.outputLines = outputLines;
                this.errorLines = errorLines;
            }

            public int ExitCode
            {
                get { return exitCode; }
            }

            public IList<string> OutputLines
            {
                get { return outputLines; }
            }

            public IList<string> ErrorLines
            {
                get { return errorLines; }
            }
        }

        public static ProcessOutput RunAndGetOutput(string filename, string arguments = "", string workingDirectory = "")
        {
            var outputLines = new List<string>();
            var errorLines = new List<string>();
            var exit = Run(filename, arguments, workingDirectory,
                outputHandler: (s, e) =>
                {
                    if (e.Data != null) // end
                    {
                        outputLines.Add(e.Data);
                    }
                },
                errorHandler: (s, e) =>
                {
                    if (e.Data != null) // end
                    {
                        errorLines.Add(e.Data);
                    }
                }
            );
            return new ProcessOutput(exit, outputLines, errorLines);
        }

    }
}