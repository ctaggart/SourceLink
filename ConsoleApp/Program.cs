using System;
using LibGit2Sharp;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var repo = new Repository(@"..\"))
            {
                if (repo.Head.IsTracking)
                {
                    foreach(var r in repo.Network.Remotes)
                    {
                        Console.WriteLine(r.Name);
                        Console.WriteLine(r.Url);
                    }
                    Console.WriteLine(repo.Head.TrackedBranch.RemoteName);
                }
            }
        }
    }
}