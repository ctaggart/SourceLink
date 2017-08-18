﻿using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SourceLink {
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "Source Code On Demand";
            app.HelpOption("-h|--help");

            app.Command("print-json", PrintJson);
            app.Command("print-documents", PrintDocuments);
            app.Command("print-urls", PrintUrls);
            app.Command("test", Test);

            if (args.Length == 0)
            {
                Console.WriteLine("SourceLink " + Version.GetAssemblyInformationalVersion());
                app.ShowHelp();
                return 0;
            }

            try
            {
                return app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }

        public static void PrintJson(CommandLineApplication command)
        {
            command.Description = "print the Source Link JSON stored in the pdb or dll";
            var pdbArgument = command.Argument("path", "set path to pdb or dll", false);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var path = pdbArgument.Value;
                if (path == null)
                {
                    command.ShowHelp();
                    return 2;
                }
                if (!File.Exists(path))
                {
                    Console.WriteLine("file does not exist: " + path);
                    return 3;
                }

                using (var drp = new DebugReaderProvider(path))
                {
                    var bytes = GetSourceLinkBytes(drp);
                    if (bytes == null || bytes.Length == 0)
                    {
                        Console.WriteLine("Source Link JSON not found in file: " + path);
                        return 4;
                    }
                    Console.WriteLine(Encoding.UTF8.GetString(bytes));
                }

                return 0;
            });
        }

        public static void PrintDocuments(CommandLineApplication command)
        {
            command.Description = "print the documents stored in the pdb or dll";
            var pdbArgument = command.Argument("path", "set path to pdb or dll", false);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var path = pdbArgument.Value;
                if (path == null)
                {
                    command.ShowHelp();
                    return 2;
                }
                if (!File.Exists(path))
                {
                    Console.WriteLine("file does not exist: " + path);
                    return 3;
                }

                using (var drp = new DebugReaderProvider(path))
                {
                    foreach (var doc in GetDocuments(drp))
                    {
                        Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                    }
                }

                return 0;
            });
        }

        public static void PrintUrls(CommandLineApplication command)
        {
            command.Description = "print the URLs for each document based on the Source Link JSON";
            var pdbArgument = command.Argument("path", "set path to pdb or dll", false);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var path = pdbArgument.Value;
                if (path == null)
                {
                    command.ShowHelp();
                    return 2;
                }
                if (!File.Exists(path))
                {
                    Console.WriteLine("file does not exist: " + path);
                    return 3;
                }

                using (var drp = new DebugReaderProvider(path))
                {
                    var missingDocs = new List<Document>();
                    foreach (var doc in GetDocumentsWithUrls(drp))
                    {
                        if (doc.IsEmbedded)
                        {
                            Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                            Console.WriteLine("embedded");
                        }
                        else if (doc.Url != null)
                        {
                            Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                            Console.WriteLine(doc.Url);
                        }
                        else
                        {
                            missingDocs.Add(doc);
                        }
                    }
                    if (missingDocs.Count > 0)
                    {
                        Console.WriteLine("" + missingDocs.Count + " Documents without URLs:");
                        foreach (var doc in missingDocs)
                        {
                            Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                        }
                        return 4;
                    }
                }

                return 0;
            });
        }

        public static int TestFile(string path)
        {
            using (var drp = new DebugReaderProvider(path))
            {
                return TestFile(drp);
            }
        }

        public static int TestFile(DebugReaderProvider drp)
        {
            var missingDocs = new List<Document>();
            var erroredDocs = new List<Document>();
            var documents = GetDocumentsWithUrlHashes(drp).GetAwaiter().GetResult();

            foreach (var doc in documents)
            {
                if (doc.IsEmbedded)
                {
                    //Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                    //Console.WriteLine("embedded");
                }
                else if (doc.Url != null)
                {
                    if (doc.Error == null)
                    {
                        //Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                        //Console.WriteLine(doc.Url);
                    }
                    else
                    {
                        erroredDocs.Add(doc);
                    }
                }
                else
                {
                    missingDocs.Add(doc);
                }
            }
            if (missingDocs.Count > 0)
            {
                Console.WriteLine("" + missingDocs.Count + " Documents without URLs:");
                foreach (var doc in missingDocs)
                {
                    Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                }
            }
            if (erroredDocs.Count > 0)
            {
                Console.WriteLine("" + erroredDocs.Count + " Documents with errors:");
                foreach (var doc in erroredDocs)
                {
                    Console.WriteLine("{0} {1} {2} {3}", doc.Hash.ToHex(), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                    Console.WriteLine(doc.Url);
                    Console.WriteLine("error: " + doc.Error);
                }
            }
            if (missingDocs.Count > 0 || erroredDocs.Count > 0)
            {
                Console.WriteLine("sourcelink test failed");
                return 4;
            }

            Console.WriteLine("sourcelink test passed: " + drp.Path);
            return 0;
        }

        public static int TestNupkg(string path, List<string> files)
        {
            var errors = 0;
            using (var p = new PackageArchiveReader(File.OpenRead(path)))
            {
                var dlls = new List<string>();
                var pdbs = new HashSet<string>();

                foreach(var f in p.GetFiles())
                {
                    switch (Path.GetExtension(f))
                    {
                        case ".dll":
                            dlls.Add(f);
                            break;
                        case ".pdb":
                            pdbs.Add(f);
                            break;
                    }
                }

                if (files.Count == 0)
                {
                    foreach (var dll in dlls)
                    {
                        var pdb = Path.ChangeExtension(dll, ".pdb");
                        if (pdbs.Contains(pdb))
                        {
                            files.Add(pdb);
                        }
                        else
                        {
                            files.Add(dll);
                        }
                    }
                }

                foreach(var file in files)
                {
                    try
                    {
                        using (var stream = p.GetStream(file))
                        using (var ms = new MemoryStream()) // seek required
                        {
                            stream.CopyTo(ms);
                            ms.Position = 0;
                            using (var drp = new DebugReaderProvider(file, ms))
                            {
                                if (TestFile(drp) != 0)
                                {
                                    Console.WriteLine("failed for " + file);
                                    errors++;
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("file not found: " + file);
                        errors++;
                    }
                }
            }
            return errors;
        }

        public static void Test(CommandLineApplication command)
        {
            command.Description = "test each URL and verify that the checksums match";
            var pathArgument = command.Argument("path", "path to pdb, dll or nupkg", false);
            var fileOption = command.Option("-f|--file <path>", "the path to a dll or pdb in a nupkg", CommandOptionType.MultipleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                var path = pathArgument.Value;
                if (path == null)
                {
                    command.ShowHelp();
                    return 2;
                }
                if (!File.Exists(path))
                {
                    Console.WriteLine("file does not exist: " + path);
                    return 3;
                }
                switch (Path.GetExtension(path))
                {
                    case ".dll":
                    case ".pdb":
                        return TestFile(path);
                    case ".nupkg":
                        var errors = TestNupkg(path, fileOption.Values);
                        if (errors > 0)
                        {
                            Console.WriteLine("{0} files did not pass in {1}", errors, path);
                            return 5;
                        }
                        return 0;
                    default:
                        return 4;
                }
            });
        }

        public static readonly Guid SourceLinkId = new Guid("CC110556-A091-4D38-9FEC-25AB9A351A6A");

        public static byte[] GetSourceLinkBytes(DebugReaderProvider drp)
        {
            var mr = drp.GetMetaDataReader();
            if (mr == null) return null;
            var blobh = default(BlobHandle);
            foreach (var cdih in mr.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
            {
                var cdi = mr.GetCustomDebugInformation(cdih);
                if (mr.GetGuid(cdi.Kind) == SourceLinkId)
                    blobh = cdi.Value;
            }
            if (blobh.IsNil) return Array.Empty<byte>();
            return mr.GetBlobBytes(blobh);
        }

        public static readonly Guid EmbeddedSourceId = new Guid("0E8A571B-6926-466E-B4AD-8AB04611F5FE");

        public static bool IsEmbedded(MetadataReader mr, DocumentHandle dh)
        {
            foreach(var cdih in mr.GetCustomDebugInformation(dh))
            {
                var cdi = mr.GetCustomDebugInformation(cdih);
                if (mr.GetGuid(cdi.Kind) == EmbeddedSourceId)
                    return true;
            }
            return false;
        }

        public static IEnumerable<Document> GetDocuments(DebugReaderProvider drp)
        {
            var mr = drp.GetMetaDataReader();
            foreach (var dh in mr.Documents)
            {
                if (dh.IsNil) continue;
                var d = mr.GetDocument(dh);
                if (d.Name.IsNil || d.Language.IsNil || d.HashAlgorithm.IsNil || d.Hash.IsNil) continue;
                yield return new Document
                {
                    Name = mr.GetString(d.Name),
                    Language = mr.GetGuid(d.Language),
                    HashAlgorithm = mr.GetGuid(d.HashAlgorithm),
                    Hash = mr.GetBlobBytes(d.Hash),
                    IsEmbedded = IsEmbedded(mr, dh)
                };
            }
        }

        public static IEnumerable<Document> GetDocumentsWithUrls(DebugReaderProvider drp)
        {
            var bytes = GetSourceLinkBytes(drp);
            if (bytes != null)
            {
                var text = Encoding.UTF8.GetString(bytes);
                var json = JsonConvert.DeserializeObject<SourceLinkJson>(text);
                foreach (var doc in GetDocuments(drp))
                {
                    if (!doc.IsEmbedded)
                        doc.Url = GetUrl(doc.Name, json);
                    yield return doc;
                }
            }
        }

        public static string GetUrl(string file, SourceLinkJson json)
        {
            if (json == null) return null;
            foreach (var key in json.documents.Keys.OrderByDescending(x => x.Split(Path.DirectorySeparatorChar).Length).ThenBy(x => x))
            {
                if (key.Contains("*"))
                {
                    var pattern = Regex.Escape(key).Replace(@"\*", "(.+)");
                    var regex = new Regex(pattern);
                    var m = regex.Match(file);
                    if (!m.Success) continue;
                    var url = json.documents[key];
                    var path = m.Groups[1].Value.Replace(@"\", "/");
                    return url.Replace("*", path);
                }
                else
                {
                    if (!key.Equals(file, StringComparison.Ordinal)) continue;
                    return json.documents[key];
                }
            }
            return null;
        }

        static async Task<IEnumerable<Document>> GetDocumentsWithUrlHashes(DebugReaderProvider drp)
        {
            // https://github.com/ctaggart/SourceLink/blob/v1/Exe/Http.fs
            // https://github.com/ctaggart/SourceLink/blob/v1/Exe/Checksums.fs

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (var hc = new HttpClient(handler))
            {
                hc.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SourceLink", "2.0.0"));
                // TODO Basic Auth support, ASCII or UTF8
                //hc.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue("Basic",
                //    Convert.ToBase64String(Encoding.ASCII.GetBytes("username:password")));

                var tasks = GetDocumentsWithUrls(drp)
                    .Select(doc => CheckDocumentHash(hc, doc))
                    .ToArray();

                await Task.WhenAll(tasks);

                return tasks.Select(x => x.Result);
            }
        }

        static async Task<Document> CheckDocumentHash(HttpClient hc, Document doc)
        {
            if (doc.Url != null)
            {
                await HashUrl(hc, doc);
                if (doc.Error == null)
                {
                    if (!doc.Hash.CollectionEquals(doc.UrlHash))
                    {
                        doc.Error = "url hash does not match: " + doc.Hash.ToHex();
                    }
                }
            }

            return doc;
        }

        // TODO async
        static async Task HashUrl(HttpClient hc, Document doc)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, doc.Url))
            using (var rsp = await hc.SendAsync(req))
            {
                if (rsp.IsSuccessStatusCode)
                {
                    using (var stream = await rsp.Content.ReadAsStreamAsync())
                    // TODO Is it more efficient to cache?
                    using (var ha = CreateHashAlgorithm(doc.HashAlgorithm))
                    {
                        doc.UrlHash = ha.ComputeHash(stream);
                    }
                }
                else
                {
                    doc.Error = "url failed " + rsp.StatusCode + ": " + rsp.ReasonPhrase;
                }
            }
        }

        // IDisposable
        public static HashAlgorithm CreateHashAlgorithm(Guid guid)
        {
            if (guid == HashAlgorithmGuids.md5) return MD5.Create();
            if (guid == HashAlgorithmGuids.sha1) return SHA1.Create();
            if (guid == HashAlgorithmGuids.sha256) return SHA256.Create();
            throw new CryptographicException("unknown HashAlgorithm " + guid);
        }
    }
}