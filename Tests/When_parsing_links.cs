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
        [InlineData("git@bitbucket.org:ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_BitBucket(string provided)
        {
            var task = new SourceLink.Create.BitBucket.CreateTask();
            Assert.Equal("https://bitbucket.org/ctaggart/sourcelink-test/raw/{commit}/*", task.ConvertUrl(provided));
        }
    }
}
