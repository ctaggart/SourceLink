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
            Assert.Equal("https://raw.githubusercontent.com/ctaggart/sourcelink-test/{commit}/*",
                SourceLink.Create.GitHub.UrlConverter.Convert(provided));
        }

        [Theory]
        [InlineData("git@gitlab.com:ctaggart/sourcelink-test.git")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test.git")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test")]
        [InlineData("https://gitlab.com/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_GitLab(string provided)
        {
            Assert.Equal("https://gitlab.com/ctaggart/sourcelink-test/raw/{commit}/*",
                 SourceLink.Create.GitLab.UrlConverter.Convert(provided));
        }

        [Theory]
        [InlineData("git@bitbucket.org:ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test.git")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test")]
        [InlineData("https://bitbucket.org/ctaggart/sourcelink-test/")]
        public void Should_return_url_in_canonical_form_for_BitBucket(string provided)
        {
            Assert.Equal("https://bitbucket.org/ctaggart/sourcelink-test/raw/{commit}/*",
                SourceLink.Create.BitBucket.UrlConverter.Convert(provided));
        }

        [Theory]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/")]
        public void Should_return_url_in_canonical_form_for_BitBucket_with_company_url(string provided)
        {
            Assert.Equal("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/raw/*?at={commit}",
                SourceLink.Create.BitBucketServer.UrlConverter.Convert(provided));
        }


        [Theory]
        [InlineData("https://bitbucket.company.com")]
        [InlineData("https://bitbucket.company.com/projects/SMN")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos")]
        [InlineData("https://bitbucket.company.com/repos/some.comapny")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/invalidPart")]
        public void Should_not_match_invalid_company_url(string provided)
        {
            Assert.NotEqual("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/raw/*?at={commit}",
                SourceLink.Create.BitBucketServer.UrlConverter.Convert(provided));
        }

        [Theory]
        [InlineData("ssh://git@internal.bitbucketserver.local:7999/sol123/reallyawesomeproject.git")]
        [InlineData("https://internal.bitbucketserver.local/scm/sol123/reallyawesomeproject.git")]
        public void Should_return_url_in_canonical_form_for_BitBucketServer(string provided)
        {
            Assert.Equal("https://internal.bitbucketserver.local/projects/sol123/repos/reallyawesomeproject/raw/*?at={commit}",
                SourceLink.Create.BitBucketServer.UrlConverter.Convert(provided));
        }
    }
}
