using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace SourceLink {
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication()
            {
                Name = "dotnet sourcelink",
                FullName = "SourceLink: Source Code On Demand",
                Description = "Source Link your Portable PDB files to allow source code to be downloaded on demand from the source code repository host"
            };
            app.HelpOption("-h|--help");

            app.Command("print-json", PrintJson);
            app.Command("print-documents", PrintDocuments);
            app.Command("print-urls", PrintUrls);
            app.Command("test-urls", TestUrls);

            if (args.Length == 0)
            {
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
            command.Description = "print the Source Link JSON stored in the Portable PDB file";
            var pdbOption = command.Option("-p|--pdb <PDB>", "set path to Porable PDB", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                if (!pdbOption.HasValue())
                {
                    command.ShowHelp();
                    return 2;
                }
                var path = pdbOption.Value();
                if (!File.Exists(path))
                {
                    Console.WriteLine("PDB file does not exist");
                    return 3;
                }

                var bytes = GetSourceLinkBytes(path);
                if (bytes.Length == 0)
                {
                    Console.WriteLine("Source Link JSON not found in PDB file");
                    return 4;
                }
                Console.WriteLine(Encoding.UTF8.GetString(bytes));

                return 0;
            });
        }

        public static void PrintDocuments(CommandLineApplication command)
        {
            command.Description = "print the documents stored in the Portable PDB file";
            var pdbOption = command.Option("-p|--pdb <PDB>", "set path to Porable PDB", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                if (!pdbOption.HasValue())
                {
                    command.ShowHelp();
                    return 2;
                }
                var path = pdbOption.Value();
                if (!File.Exists(path))
                {
                    Console.WriteLine("PDB file does not exist");
                    return 3;
                }

                foreach (var doc in GetDocuments(path))
                {
                    Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                }

                return 0;
            });
        }

        public static void PrintUrls(CommandLineApplication command)
        {
            command.Description = "print the URLs for each document based on the Source Link JSON";
            var pdbOption = command.Option("-p|--pdb <PDB>", "set path to Porable PDB", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                if (!pdbOption.HasValue())
                {
                    command.ShowHelp();
                    return 2;
                }
                var path = pdbOption.Value();
                if (!File.Exists(path))
                {
                    Console.WriteLine("PDB file does not exist");
                    return 3;
                }

                var missingDocs = new List<Document>();
                foreach (var doc in GetDocumentsWithUrls(path))
                {
                    if (doc.Url != null)
                    {
                        Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
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
                        Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                    }
                    return 4;
                }

                return 0;
            });
        }

        public static void TestUrls(CommandLineApplication command)
        {
            command.Description = "test each URL and verify that the checksums from the Portable PDB match";
            var pdbOption = command.Option("-p|--pdb <PDB>", "set path to Porable PDB", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                if (!pdbOption.HasValue())
                {
                    command.ShowHelp();
                    return 2;
                }
                var path = pdbOption.Value();
                if (!File.Exists(path))
                {
                    Console.WriteLine("PDB file does not exist");
                    return 3;
                }

                var missingDocs = new List<Document>();
                var erroredDocs = new List<Document>();
                foreach (var doc in GetDocumentsWithUrlHashes(path))
                {
                    if (doc.Url != null)
                    {
                        if(doc.Error == null)
                        {
                            Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                            Console.WriteLine(doc.Url);
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
                        Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                    }
                }
                if (erroredDocs.Count > 0)
                {
                    Console.WriteLine("" + erroredDocs.Count + " Documents with errors:");
                    foreach (var doc in erroredDocs)
                    {
                        Console.WriteLine("{0} {1} {2} {3}", toHex(doc.Hash), HashAlgorithmGuids.GetName(doc.HashAlgorithm), LanguageGuids.GetName(doc.Language), doc.Name);
                        Console.WriteLine(doc.Url);
                        Console.WriteLine("error: " + doc.Error);
                    }
                }
                if (missingDocs.Count > 0 || erroredDocs.Count > 0)
                    return 4;

                return 0;
            });
        }

        public static readonly Guid SourceLinkId = new Guid("CC110556-A091-4D38-9FEC-25AB9A351A6A");

        public static byte[] GetSourceLinkBytes(string pdb)
        {
            using (var mrp = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdb)))
            {
                var mr = mrp.GetMetadataReader();
                var blobh = default(BlobHandle);
                foreach (var cdih in mr.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
                {
                    var cdi = mr.GetCustomDebugInformation(cdih);
                    if (mr.GetGuid(cdi.Kind) == SourceLinkId)
                    {
                        blobh = cdi.Value;
                    }
                }
                if (blobh.IsNil) return Array.Empty<byte>();
                return mr.GetBlobBytes(blobh);
            }
        }

        public static IEnumerable<Document> GetDocuments(string pdb)
        {
            using (var mrp = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdb)))
            {
                var mr = mrp.GetMetadataReader();
                foreach (var dh in mr.Documents)
                {
                    var d = mr.GetDocument(dh);
                    yield return new Document
                    {
                        Name = mr.GetString(d.Name),
                        Language = mr.GetGuid(d.Language),
                        HashAlgorithm = mr.GetGuid(d.HashAlgorithm),
                        Hash = mr.GetBlobBytes(d.Hash),
                    };
                }
            }
        }

        public static string toHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        public static IEnumerable<Document> GetDocumentsWithUrls(string pdb)
        {
            var bytes = GetSourceLinkBytes(pdb);
            var text = Encoding.UTF8.GetString(bytes);
            var json = JsonConvert.DeserializeObject<SourceLinkJson>(text);
            foreach (var doc in GetDocuments(pdb))
            {
                doc.Url = GetUrl(doc.Name, json);
                yield return doc;
            }
        }

        public static string GetUrl(string file, SourceLinkJson json)
        {
            foreach (var key in json.documents.Keys)
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

        static IEnumerable<Document> GetDocumentsWithUrlHashes(string pdb)
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

                foreach (var doc in GetDocumentsWithUrls(pdb))
                {
                    if(doc.Url != null)
                    {
                        HashUrl(hc, doc);
                        if (doc.Error != null) continue;
                        if (!doc.Hash.CollectionEquals(doc.UrlHash))
                        {
                            doc.Error = "url hash does not match: " + toHex(doc.Hash);
                        }
                    }
                    yield return doc;
                }
            }
        }

        // TODO async
        static void HashUrl(HttpClient hc, Document doc)
        {
            using (var req = new HttpRequestMessage(HttpMethod.Get, doc.Url))
            {
                using (var rsp = hc.SendAsync(req).Result)
                {
                    if (rsp.IsSuccessStatusCode)
                    {
                        using (var stream = rsp.Content.ReadAsStreamAsync().Result)
                        {
                            // TODO Is it more efficient to cache?
                            using(var ha = CreateHashAlgorithm(doc.HashAlgorithm))
                            {
                                doc.UrlHash = ha.ComputeHash(stream);
                            }
                        }
                    }
                    else
                    {
                        doc.Error = "url failed " + rsp.StatusCode + ": " + rsp.ReasonPhrase;
                    }
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