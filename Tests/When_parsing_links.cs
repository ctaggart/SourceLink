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
        [InlineData("https://fabrikam.visualstudio.com/SomeProject/_git/SomeRepo")]
        [InlineData("https://fabrikam.visualstudio.com/DefaultCollection/SomeProject/_git/SomeRepo")]
        public void Should_return_url_in_canonical_form_for_VSTS(string provided)
        {
            var task = new SourceLink.Create.VSTS.CreateTask();
            Assert.Equal("https://fabrikam.visualstudio.com/DefaultCollection/_apis/git/SomeProject/repositories/SomeRepo/items?api-version=1.0&versionType=commit&version={commit}&scopePath=*", task.ConvertUrl(provided));
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
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/")]
        public void Should_return_url_in_canonical_form_for_BitBucket_with_company_url(string provided)
        {
            var task = new SourceLink.Create.BitBucketServer.CreateTask();
            Assert.Equal("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/raw/*?at={commit}", task.ConvertUrl(provided));
        }


        [Theory]
        [InlineData("https://bitbucket.company.com")]
        [InlineData("https://bitbucket.company.com/projects/SMN")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos")]
        [InlineData("https://bitbucket.company.com/repos/some.comapny")]
        [InlineData("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/invalidPart")]
        public void Should_not_match_invalid_company_url(string provided)
        {
            var task = new SourceLink.Create.BitBucketServer.CreateTask();
            Assert.NotEqual("https://bitbucket.company.com/projects/SMN/repos/company.innernamespace.project/raw/*?at={commit}", task.ConvertUrl(provided));
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
