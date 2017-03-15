using Microsoft.Extensions.CommandLineUtils;
using System;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace SourceLink.Git {
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Source Code On Demand";
            app.HelpOption("-h|--help");

            app.Command("repo", PrintRepo);
            app.Command("origin", PrintOrigin);
            app.Command("create", Create);

            if (args.Length == 0)
            {
                Console.WriteLine("SourceLink Git " + Version.GetAssemblyInformationalVersion());
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
                var dir = ".";
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
                var dir = ".";
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
            //var embedOption = command.Option("-e|--embed <file>", "the sourcelink.embed file to write", CommandOptionType.SingleValue);
            var urlOption = command.Option("-u|--url <url>", "URL for downloading the source files, use {0} for commit and * for path", CommandOptionType.SingleValue);
            var sourceOption = command.Option("-s|--source <url>", "source file to verify checksum in git repository", CommandOptionType.MultipleValue);
            var notInGitOption = command.Option("--notingit <option>", "embed, warn, or error when a source file is not in git. embed is default", CommandOptionType.SingleValue);
            var hashMismatchOption = command.Option("--hashmismatch <option>", "embed, warn, or error when a source file hash does not match git. embed is default", CommandOptionType.SingleValue);
            var noAutoLfOption = command.Option("--noautolf", "disable changing the line endings to match the git repository", CommandOptionType.NoValue);

            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var notInGitOpt = "embed";
                if (notInGitOption.HasValue())
                    notInGitOpt = notInGitOption.Value();

                var hashMismatchOpt = "embed";
                if (hashMismatchOption.HasValue())
                    hashMismatchOpt = hashMismatchOption.Value();

                var noAutoLfOpt = noAutoLfOption.HasValue();

                var filesNotInGit = new List<SourceFile>();
                var filesFixedLineEndings = new List<SourceFile>();
                var filesHashMismatch = new List<SourceFile>();
                var embedFiles = new List<SourceFile>();
                var errors = 0;

                var dir = ".";
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

                if (sourceOption.HasValue())
                {
                    var files = sourceOption.Values.Select(source => new SourceFile { FilePath = source });

                    using (var repo = new Repository(repoPath))
                    using (var sha1 = SHA1.Create())
                    {
                        foreach (var sf in files)
                        {
                            sf.GitPath = GetGitPath(repoPath, sf.FilePath);
                            if (sf.GitPath == null)
                            {
                                filesNotInGit.Add(sf);
                            }
                            else
                            {
                                var index = repo.Index[sf.GitPath];
                                if (index == null)
                                {
                                    filesNotInGit.Add(sf);
                                }
                                else if (index.Path != sf.GitPath)
                                {
                                    // mysysgit sets core.ignorecase true by default
                                    // but most web sites like GitHub are case sensitive
                                    filesNotInGit.Add(sf);
                                }
                                else
                                {
                                    sf.GitHash = index.Id.Sha;
                                    sf.FileHash = ComputeGitHash(sha1, File.ReadAllBytes(sf.FilePath)).ToHex();
                                    if (!HashesMatch(sf.FileHash, sf.GitHash))
                                    {
                                        if (!noAutoLfOpt)
                                        {
                                            if (TryFixLineEndings(sha1, sf))
                                            {
                                                filesFixedLineEndings.Add(sf);
                                            }
                                            else
                                            {
                                                filesHashMismatch.Add(sf);
                                            }
                                        }
                                        else
                                        {
                                            filesHashMismatch.Add(sf);
                                        }
                                    }

                                }
                            }
                        }
                    }

                    foreach (var sf in filesFixedLineEndings)
                    {
                        Console.WriteLine("fixed line endings for " + sf.FilePath);
                    }

                    foreach (var sf in filesNotInGit)
                    {
                        switch (notInGitOpt)
                        {
                            case "error":
                                Console.WriteLine("error: file not in git: " + sf.FilePath);
                                errors++;
                                break;
                            case "warn":
                                Console.WriteLine("warning: file not in git: " + sf.FilePath);
                                break;
                            default:
                                Console.WriteLine("embedding file not in git: " + sf.FilePath);
                                embedFiles.Add(sf);
                                break;
                        }
                    }

                    foreach(var sf in filesHashMismatch)
                    {
                        switch (hashMismatchOpt)
                        {
                            case "error":
                                Console.WriteLine("error: hash mismatch for: " + sf.FilePath);
                                errors++;
                                break;
                            case "warn":
                                Console.WriteLine("warning: hash mismatch for: " + sf.FilePath);
                                break;
                            default:
                                Console.WriteLine("embedding file due to hash mismatch: " + sf.FilePath);
                                embedFiles.Add(sf);
                                break;
                        }
                    }
                }

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

                //var embedFile = embedOption.Value();
                //if (String.IsNullOrEmpty(embedFile))
                var embedFile = Path.ChangeExtension(file, ".embed");
                if (embedFiles.Count > 0)
                {
                    Console.WriteLine("embedding " + embedFiles.Count + " source files");
                    using (var sw = new StreamWriter(File.OpenWrite(embedFile)))
                    {
                        foreach (var sf in embedFiles)
                        {
                            sw.WriteLine(sf.FilePath);
                        }
                    }
                }
                else
                {
                    if (File.Exists(embedFile))
                        File.Delete(embedFile);
                }

                return errors == 0 ? 0 : 1;
            });
        }

        public static bool TryFixLineEndings(SHA1 sha1, SourceFile sf)
        {
            var fileBytes = new byte[] { };
            var hashesMatch = false;

            // https://github.com/ctaggart/SourceLink/blob/v1/Exe/LineFeed.fs#L31-L50
            // passing UTFEncoding without the BOM set allows it to be detected
            // http://stackoverflow.com/a/27976558/23059
            using (var fs = File.OpenRead(sf.FilePath))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                var text = sr.ReadToEnd();
                var lines = text.Split(new char[]{'\n'});
                if (lines.Length > 0 && HasCrLf(lines))
                {
                    using (var ms = new MemoryStream())
                    {
                        // check hash without carriage return
                        // if the hash matches, overwrite file
                        using (var sw = new StreamWriter(ms, sr.CurrentEncoding))
                        {
                            for (var i = 0; i < lines.Length - 1; i++)
                            {
                                sw.Write(lines[i].TrimEnd(new char[] { '\r' }));
                                sw.Write('\n');
                            }
                            sw.Write(lines[lines.Length - 1].TrimEnd(new char[] { '\r' }));
                        }
                        fileBytes = ms.ToArray();
                        var fileHash = ComputeGitHash(sha1, fileBytes);
                        hashesMatch = HashesMatch(sf.GitHash, fileHash.ToHex());
                    }
                }
            }

            if (hashesMatch)
                File.WriteAllBytes(sf.FilePath, fileBytes);
            return hashesMatch;
        }

        public static bool HasCrLf(string[] lines)
        {
            foreach(var line in lines)
            {
                if (line.EndsWith("\r"))
                    return true;
            }
            return false;
        }

        public static bool HashesMatch(string fileHash, string gitHash)
        {
            if (fileHash == null || fileHash == null) return false;
            return fileHash.Equals(gitHash);
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
                {
                    return d;
                }
            }
            return null;
        }

        public static string GetOrigin(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                foreach (var r in repo.Network.Remotes)
                {
                    if (r.Name == "origin")
                    {
                        return r.Url;
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

        public static byte[] ComputeGitHash (SHA1 sha1, byte[] file)
        {
            // https://github.com/ctaggart/SourceLink/blob/v1/Git/GitRepo.fs#L31
            // let checksum = sha1.ComputeHash(Byte.concat prefix.ToUtf8 bytes) |> Hex.encode
            var prefix = string.Format("blob {0}{1}", file.Length, '\0');
            return sha1.ComputeHash(ConcatBytes(Encoding.UTF8.GetBytes(prefix), file));
        }

        public static byte[] ConcatBytes(byte[] a, byte[] b)
        {
            // https://github.com/ctaggart/SourceLink/blob/v1/SourceLink/SystemExtensions.fs#L144
            var c = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, c, 0, a.Length);
            Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
            return c;
        }

        public static string GetGitPath(string repo, string file)
        {
            var path = Path.GetFullPath(file);
            if (!path.StartsWith(repo)) return null;
            return path.Substring(repo.Length + 1);
        }
    }
}