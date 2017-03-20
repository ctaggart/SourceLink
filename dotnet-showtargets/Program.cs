using System;
using System.IO;
using Microsoft.Build.Evaluation;
using System.Collections.Generic;

namespace dotnet_showtargets
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("pass in the project as the first argument");
                return 1;
            }
            var file = args[0];
            if (!File.Exists(file))
            {
                Console.WriteLine("can't find the project file");
                return 2;
            }

            // https://github.com/ctaggart/SourceLink/blob/v1/SourceLink/VsProj.fs



            //var globalProps = new Dictionary<string, string>() {
            //    { "MSBuildBinPath", @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin" }
            //};
            var pc = new ProjectCollection(ToolsetDefinitionLocations.Default);

            var props = new Dictionary<string, string>()
            {
            };
            var p = new Project(file, props, null, pc);

            var path = "";
            foreach(var t in p.Targets)
            {
                if (path != t.Value.FullPath)
                {
                    Console.WriteLine();
                    Console.WriteLine(t.Value.FullPath);
                }
                Console.WriteLine("  " + t.Key);
                path = t.Value.FullPath;
            }

            return 0;
        }
    }
}