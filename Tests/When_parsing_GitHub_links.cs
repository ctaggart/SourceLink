using SourceLink.Create.GitHub;
using Xunit;

namespace Tests
{
    public class When_parsing_GitHub_links
    {
        [Theory]
        [InlineData("git@github.com:ctaggart/sourcelink-test.git")]
        [InlineData("https://github.com/ctaggart/sourcelink-test.git")]
        [InlineData("https://github.com/ctaggart/sourcelink-test")]
        public void Should_return_url_in_canonical_form(string provided)
        {
            var task = new CreateTask();
            Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{commit}/*", task.ConvertUrl(provided));
        }

    }
}
