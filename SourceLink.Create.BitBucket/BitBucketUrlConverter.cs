
using System.Text.RegularExpressions;

namespace SourceLink.Create.BitBucket
{
    public static class UrlConverter
    {
        public static string Convert(string origin)
        {
            // Process as Bitbucket Server url if not from bitbucket.org
            if (!origin.Contains("bitbucket.org")) return ConvertServer(origin);
            if (origin.StartsWith("git@"))
            {
                origin = origin.Replace(':', '/');
                origin = origin.Replace("git@", "https://");
            }
            origin = origin.Replace(".git", "");
            var uri = new System.Uri(origin);
            return "https://bitbucket.org" + uri.LocalPath + "/raw/{commit}/*";
        }

        public static string ConvertServer(string origin)
        {
            // Try match https:// url.
            var match = Regex.Match(origin, @"^https:\/\/(.*)\/scm\/(.*)\/(.*)\.git$", RegexOptions.IgnoreCase);
            if (match.Success == false)
            {
                // Try match ssh://git url.
                match = Regex.Match(origin, @"^ssh:\/\/git@(.*):\d+\/(.*)\/(.*)\.git$", RegexOptions.IgnoreCase);
            }

            // Any match?
            if (match.Success)
            {
                return $"https://{match.Groups[1]}/projects/{match.Groups[2]}/repos/{match.Groups[3]}/raw/*?at={{commit}}";
            }
            else
            {
                return null;
            }
        }
    }
}
