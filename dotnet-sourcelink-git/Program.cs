using Microsoft.Extensions.CommandLineUtils;
using System;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;

namespace SourceLink.Git {
    public class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication()
            {
                Name = "dotnet sourcelink-git",
            };
            app.HelpOption("-h|--help");

            app.Command("repo", PrintRepo);
            app.Command("origin", PrintOrigin);
            app.Command("create", Create);

            if (args.Length == 0)
            {
                app.ShowHelp();
                return 0;
            }
            app.Execute(args);
            return 0;
        }

        public static void PrintRepo(CommandLineApplication command)
        {
            command.Description = "prints the repository path"; // and nothing else
            var dirOption = command.Option("-d|--dir <directory>", "the directory to look for the repository", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var dir = "./";
                if (dirOption.HasValue())
                    dir = dirOption.Value();

                var repoPath = FindGitRepo(dir);
                if (repoPath == null)
                {
                    Console.Error.WriteLine("repository not found at or above " + dir);
                    return 1;
                }

                Console.WriteLine(repoPath);
                return 0;
            });
        }

        public static void PrintOrigin(CommandLineApplication command)
        {
            command.Description = "prints the git repository url for origin"; // and nothing else
            var dirOption = command.Option("-d|--dir <directory>", "the directory to look for the repository", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var dir = "./";
                if (dirOption.HasValue())
                    dir = dirOption.Value();

                var repoPath = FindGitRepo(dir);
                if (repoPath == null) {
                    Console.Error.WriteLine("repository not found at or above " + dir);
                    return 1;
                }

                var origin = GetOrigin(repoPath);
                if(origin == null)
                {
                    Console.Error.WriteLine("origin not found");
                    return 2; // not found
                }

                Console.WriteLine(origin);
                return 0;
            });
        }

        public static void Create(CommandLineApplication command)
        {
            command.Description = "creates the Source Link JSON file";
            var dirOption = command.Option("-d|--dir <directory>", "the directory to look for the repository", CommandOptionType.SingleValue);
            var fileOption = command.Option("-f|--file <file>", "file to write", CommandOptionType.SingleValue);
            var urlOption = command.Option("-u|--url <url>", "URL for downloading the source files, use {0} for commit and * for path", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {

                // get commit

                // TODO write actual json
                using (var sw = System.IO.File.CreateText(fileOption.Value()))
                {
                    sw.WriteLine("{\"documents\": { \"C:\\\\Users\\\\camer\\\\cs\\\\sourcelink-test\\\\*\" : \"https://raw.githubusercontent.com/ctaggart/sourcelink-test/b5012a98bed12f6704cb942e92ba34ccdbd920d8/*\" }}");
                }
                return 0;
            });
        }

        // https://github.com/ctaggart/SourceLink/blob/v1/SourceLink/SystemExtensions.fs#L115
        public static IEnumerable<string> GetDirectoryNames(string path)
        {
            path = Path.GetFullPath(path);
            if (Directory.Exists(path))
                yield return path;
            var parent = Path.GetDirectoryName(path);
            while (!String.IsNullOrEmpty(parent))
            {
                yield return parent;
                parent = Path.GetDirectoryName(parent);
            }
        }

        public static string FindGitRepo(string dir)
        {
            foreach(var d in GetDirectoryNames(dir))
            {
                if (Repository.IsValid(d))
                    return d;
            }
            return null;
        }

        public static string GetOrigin(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                if (repo.Head.IsTracking)
                {
                    foreach (var r in repo.Network.Remotes)
                    {
                        if (r.Name == "origin")
                        {
                            return r.Url;
                        }
                    }
                }
            }
            return null;
        }

    }
}