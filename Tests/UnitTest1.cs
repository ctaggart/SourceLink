using Xunit;
//using SourceLink.Create.GitHub;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ParseGitHubUrl()
        {
            //// https
            //Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{0}/*",
            //    CreateTask.GetRepoUrl("https://github.com/ctaggart/sourcelink-test.git"));

            //// no trailing .git
            //Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{0}/*",
            //    CreateTask.GetRepoUrl("https://github.com/ctaggart/sourcelink-test"));

            //// git
            //Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{0}/*",
            //    CreateTask.GetRepoUrl("git@github.com:ctaggart/sourcelink-test.git"));

            //// trailing slash
            //Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{0}/*",
            // CreateTask.GetRepoUrl("https://github.com/ctaggart/sourcelink-test/"));
        }
    }
}
