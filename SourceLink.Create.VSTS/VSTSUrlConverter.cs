using System.Text.RegularExpressions;

namespace SourceLink.Create.VSTS
{
    public static class UrlConverter
    {
        private static Regex VSTSUriMatcher = new Regex("https://(?<tenant>\\w+).visualstudio.com/(DefaultCollection/)?(?<project>\\w+)/_git/(?<repo>\\w+)", RegexOptions.IgnoreCase);
        public static string Convert(string origin)
        {
            var match = VSTSUriMatcher.Match(origin);
            if (match == null)
                return null;

            return $"https://{match.Groups["tenant"].Value}.visualstudio.com/DefaultCollection/_apis/git/{match.Groups["project"].Value}/repositories/{match.Groups["repo"].Value}/items?api-version=1.0&versionType=commit&version={{commit}}&scopePath=*";
        }
    }
}
