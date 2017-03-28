
namespace SourceLink.Create.BitBucket
{
    public static class UrlConverter
    {
        public static string Convert(string origin)
        {
            if (!origin.Contains("bitbucket.org")) return null;
            if (origin.StartsWith("git@"))
            {
                origin = origin.Replace(':', '/');
                origin = origin.Replace("git@", "https://");
            }
            origin = origin.Replace(".git", "");
            var uri = new System.Uri(origin);
            return "https://bitbucket.org" + uri.LocalPath + "/raw/{commit}/*";
        }
    }
}
