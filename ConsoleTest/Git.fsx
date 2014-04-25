#r "../packages/LibGit2Sharp.0.17.0.0/lib/net35/LibGit2Sharp.dll"

open LibGit2Sharp

let repo = new Repository(@"C:\Projects\SourceLink")
repo.Head.Tip.Sha