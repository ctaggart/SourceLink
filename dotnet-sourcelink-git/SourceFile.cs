using System.Text;

namespace SourceLink.Git
{
    public class SourceFile
    {
        public string FilePath { get; set; }
        public string GitPath { get; set; }
        public string GitHash { get; set; }
        public string FileHash { get; set; }
    }
}
