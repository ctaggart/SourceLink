using Microsoft.Extensions.CommandLineUtils;
using System;
using LibGit2Sharp;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandLineApplication()
        {
            Name = "dotnet sourcelink-git",
        };
        app.HelpOption("-h|--help");

        app.Command("repo", command =>
        {
            command.Description = "prints the git repository url";
            var dirOption = command.Option("-d|--dir <directory>", "the directory to begin looking", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                using (var repo = new Repository(@".\"))
                {
                    if (repo.Head.IsTracking)
                    {
                        foreach (var r in repo.Network.Remotes)
                        {
                            Console.WriteLine(r.Name);
                            Console.WriteLine(r.Url);
                        }
                        Console.WriteLine(repo.Head.TrackedBranch.RemoteName);
                    }
                }
                return 0;
            });
        });

        if (args.Length == 0)
        {
            app.ShowHelp();
            return 0;
        }
        app.Execute(args);
        return 0;
    }

}