using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;
using IO = System.IO;
using System.Collections.Generic;

namespace SourceLink.Create.CommandLine
{
    public class CreateTask : MSBuildTask
    {
        [Required]
        public string RootDirectory { get; set; }

        public string Url { get; set; }

        public string OriginUrl { get; set; }

        [Required]
        public string Commit { get; set; }

        [Required]
        public string File { get; set; }

        public string ServerType { get;set; }

        [Output]
        public string SourceLink { get; set; }

        public override bool Execute()
        {
            var url = Url;
            if(url == null)
            {
                if(OriginUrl == null)
                {
                    Log.LogError("OriginUrl not set");
                    return false;
                }
                url = ConvertUrl(OriginUrl, ServerType);
                Log.LogMessage(MessageImportance.Normal, "SourceLinkUrl: " + url);
                if (url == null)
                {
                    Log.LogError("unable to convert OriginUrl: " + OriginUrl);
                    return false;
                }
            }

            var rootDirectory = IO.Path.GetFullPath(RootDirectory);
            if (!rootDirectory.EndsWith("" + IO.Path.DirectorySeparatorChar))
                rootDirectory += IO.Path.DirectorySeparatorChar;
            rootDirectory += '*';
            rootDirectory = rootDirectory.Replace(@"\", @"\\"); // json escape

            using (var json = new IO.StreamWriter(IO.File.OpenWrite(File)))
            {
                json.Write("{\"documents\":{\"");
                json.Write(rootDirectory);
                json.Write("\":\"");
                json.Write(url.Replace("{commit}", Commit));
                json.Write("\"}}");
            }

            SourceLink = File;
            return true;
        }

        public delegate string UrlConverter(string url);

        public static string ConvertUrl(string origin, string serverType)
        {
            var urlConverters = new List<UrlConverter>();

            switch (serverType)
            {
                case "GitHub":
                    urlConverters.Add(GitHub.UrlConverter.Convert);
                    break;
                case "BitBucket":
                    urlConverters.Add(BitBucket.UrlConverter.Convert);
                    break;
                case "BitBucketServer":
                    urlConverters.Add(BitBucketServer.UrlConverter.Convert);
                    break;
                default:
                    urlConverters.Add(GitHub.UrlConverter.Convert);
                    urlConverters.Add(BitBucket.UrlConverter.Convert);
                    break;
            }

            foreach(var urlConverter in urlConverters)
            {
                var url = urlConverter(origin);
                if (url != null)
                    return url;
            }

            return null;
        }

    }
}
