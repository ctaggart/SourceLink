using System;
using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace SourceLink
{
    public class ShowTargetsTask : MSBuildTask
    {
        public override bool Execute()
        {
            // http://stackoverflow.com/a/484528/23059
            //var project = new Project();
            //project.Load(@"c:\path\to\my\project.proj");
            //foreach (Target target in project.Targets)
            //{
            //    Console.WriteLine("{0}", target.Name);
            //}

            return true;
        }
    }
}
