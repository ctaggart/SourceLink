# Integrate with GitHub

SourceLink is used to source index many open source projects on GitHub. The `--url` to use when indexing should use `raw.githubusercontent.com`. It [was changed](https://developer.github.com/changes/2014-04-25-user-content-security/) from `raw.github.com` in April 2014, but the old URLs will still work with the redirects in place. Here are some example `--url` arguments:

    'https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%'
    'https://raw.githubusercontent.com/octokit/octokit.net/{0}/%var2%'
    'https://raw.githubusercontent.com/Microsoft/visualfsharp/{0}/%var2%'

# Private repositories
Support for private repositories was added for SourceLink 1.0. Documentation coming...
