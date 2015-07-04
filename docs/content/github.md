# Source Index to GitHub

SourceLink is used to source index many open source projects on GitHub. The `--url` to use when indexing should use `raw.githubusercontent.com`. It [was changed](https://developer.github.com/changes/2014-04-25-user-content-security/) from `raw.github.com` in April 2014, but the old URLs will still work with the redirects in place. Here are some example `--url` arguments:

    'https://raw.githubusercontent.com/ctaggart/SourceLink/{0}/%var2%'
    'https://raw.githubusercontent.com/octokit/octokit.net/{0}/%var2%'
    'https://raw.githubusercontent.com/Microsoft/visualfsharp/{0}/%var2%'

## Private repositories
Support for private repositories was added for SourceLink 1.0. Unfortunately, GitHub returns an HTTP 404 Not Found instead of the normal HTTP 401 Unauthorized  when authentication is required. To fix this, I created a very simple [60-line](https://github.com/ctaggart/SourceLink-Proxy/blob/master/server.ts) web service Node.js app in TypeScript. It even comes with Application Insights. It may be cloned and deployed to your server. I have an instance of it hosted on a small Azure App Service plan.

![](https://cloud.githubusercontent.com/assets/80104/8506354/a4e8fa6e-21c4-11e5-9f13-5ee8abbd9d64.png)

It is on a $56/mo plan that my Microsoft .NET MVP covers. :)

![](https://cloud.githubusercontent.com/assets/80104/8506413/9a9dd964-21c7-11e5-9346-c94110cb1ddd.png)

My hope is that AppVeyor and other companies will add this as a Source Service similar to AppVeyor's awesome [NuGet Service](http://www.appveyor.com/docs/nuget). My company would gladly pay a bit extra for this instead of hosting it ourselves.

To source index with the proxy, just change the url to be:

    'https://sourcelink.azurewebsites.net/Company/PrivateRepo/{0}/%var2%'

However, I recommend that you create a DNS CNAME for indexing your private code in case you wish to change the source server and host your own. You can name the subdomain anything you wish.

    'https://raw.company.com/Company/PrivateRepo/{0}/%var2%'

### Check the pdb files

You may verify that the pdb files are source indexed and you are able to download the source files using `SourceLink.exe`. Version 1.0 added authentication for `checksums --check`.

![](https://cloud.githubusercontent.com/assets/80104/8490571/17833b10-20e0-11e5-85ac-ccd99907baf0.png)

```
SourceLink.exe checksums --pdb bin/Your.pdb --check --username ctaggart --password n0tr3al
```

If hou have GitHub [Two-Factor Authentication](https://help.github.com/articles/about-two-factor-authentication/) enabled, just use a [personal access token](https://help.github.com/articles/creating-an-access-token-for-command-line-use/) for `--username`:

```
SourceLink.exe checksums --pdb bin/Your.pdb --check --username 6d2e65d6bdaeb91249a024e9201d845b5ef0beb3
```
