#r "bin\Release\LibGit2Sharp.dll"

open LibGit2Sharp

let repo = new Repository(@"C:\Projects\SourceLink")
repo.Head.Tip.Sha