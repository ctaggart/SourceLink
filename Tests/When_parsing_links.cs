using Xunit;

namespace Tests
{
    public class When_parsing_links
    {
        [Theory]
        [InlineData("git@github.com:ctaggart/sourcelink-test.git")]
        [InlineData("https://github.com/ctaggart/sourcelink-test.git")]
        [InlineData("https://github.com/ctaggart/sourcelink-test")]
        [InlineData("https://github.com/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_GitHub(string provided)
        {
            var task = new SourceLink.Create.GitHub.CreateTask();
            Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{commit}/*", task.ConvertUrl(provided));
        }

        [Theory]
        [InlineData("git@gitlab.com:ctaggart/sourcelink-test.git")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test.git")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_GitLab(string provided)
        {
            var task = new SourceLink.Create.GitLab.CreateTask();
            Assert.Equal("https://gitlab.com/ctaggart/sourcelink-test/raw/{commit}/*", task.ConvertUrl(provided));
        }

        [Theory]
        [InlineData("git@bitbucket.org:ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_BitBucket(string provided)
        {
            var task = new SourceLink.Create.BitBucket.CreateTask();
            Assert.Equal("https://bitbucket.org/ctaggart/sourcelink-test/raw/{commit}/*", task.ConvertUrl(provided));
        }

        [Theory]
        [InlineData("ssh://git@internal.bitbucketserver.local:7999/sol123/reallyawesomeproject.git")]
        [InlineData("https://internal.bitbucketserver.local/scm/sol123/reallyawesomeproject.git")]
        public void Should_return_url_in_canonical_form_for_BitBucketServer(string provided)
        {
            var task = new SourceLink.Create.BitBucketServer.CreateTask();
            Assert.Equal("https://internal.bitbucketserver.local/projects/sol123/repos/reallyawesomeproject/raw/*?at={commit}", task.ConvertUrl(provided));
        }
    }
}
