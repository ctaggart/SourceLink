namespace SourceLink.Create.GitLab
{
    public static class UrlConverter
    {
        public static string Convert(string origin)
        {
            if (origin.StartsWith("git@"))
            {
                origin = origin.Replace(':', '/');
                origin = origin.Replace("git@", "https://");
            }

            origin = origin.Replace(".git", string.Empty);
            origin = origin.TrimEnd('/');
            var uri = new System.Uri(origin);

            return $"https://{uri.Authority}{uri.LocalPath}/raw/{{commit}}/*";
        }
    }
}
