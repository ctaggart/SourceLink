using Microsoft.Extensions.CommandLineUtils;
using System;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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

            try
            {
                return app.Execute(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
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
            var dirOption = command.Option("-d|--dir <directory>", "the directory to look for the git repository", CommandOptionType.SingleValue);
            var fileOption = command.Option("-f|--file <file>", "the sourcelink.json file to write", CommandOptionType.SingleValue);
            var embedOption = command.Option("-e|--embed <file>", "the sourcelink.embed file to write", CommandOptionType.SingleValue);
            var urlOption = command.Option("-u|--url <url>", "URL for downloading the source files, use {0} for commit and * for path", CommandOptionType.SingleValue);
            var sourceOption = command.Option("-s|--source <url>", "source file to verify checksum in git repository", CommandOptionType.MultipleValue);
            
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

                if (!fileOption.HasValue())
                {
                    Console.Error.WriteLine("--file option required");
                    return 2;
                }
                var file = fileOption.Value();

                if (!urlOption.HasValue())
                {
                    Console.Error.WriteLine("--url option required");
                    return 3;
                }
                var url = urlOption.Value();
                var commit = GetCommit(repoPath);
                url = url.Replace("{commit}", commit);

                // TODO test checksums
                //if (sourceOption.HasValue())
                //{
                //    var n = sourceOption.Values.Count;

                //    if (embedOption.HasValue())
                //    {
                //        var embed = embedOption.Value();

                //        using (var sw = new StreamWriter(File.OpenWrite(file)))
                //        {
                //            sw.Write("a.cs;b.cs;");
                //        }
                //    }
                //}

                var json = new SourceLinkJson
                {
                    documents = new Dictionary<string, string>
                    {
                        { string.Format("{0}{1}{2}", repoPath, Path.DirectorySeparatorChar, '*'), url },
                    }
                };

                using (var sw = new StreamWriter(File.OpenWrite(file)))
                {
                    var js = new JsonSerializer();
                    js.Serialize(sw, json);
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

        public static string GetCommit(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                return repo.Head.Tip.Sha;
            }
        }

    }
}