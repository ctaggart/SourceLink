
namespace SourceLink.Create.GitHub
{
    public static class UrlConverter
    {
        public static string Convert(string origin)
        {
            if (!origin.Contains("github.com")) return null;
            if (origin.StartsWith("git@"))
            {
                origin = origin.Replace(':', '/');
                origin = origin.Replace("git@", "https://");
            }
            origin = origin.Replace(".git", "");
            var uri = new System.Uri(origin);
            return "https://raw.githubusercontent.com" + uri.LocalPath + "/{commit}/*";
        }
    }
}
