# SourceLink.exe

SourceLink.exe is distributed through a NuGet package available in both [the NuGet Gallery](https://www.nuget.org/packages/SourceLink) and [the Chocolatey Gallery](https://chocolatey.org/packages/SourceLink). The simplest way to get SourceLink.exe is by using Chocolatey.

To install Chocolatey using PowerShell as Administrator:

    iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))

The install SourceLink:

    choco install SourceLink

With SourceLink.exe installed, simply run it to see the commands available:

![SourceLink.exe help](https://cloud.githubusercontent.com/assets/80104/6773015/2f6eda22-d0c7-11e4-8ffa-b92f2f1db099.png)

## Index

The `index` command is the most important one. Typical usage looks like this:

![SourceLink.exe index LibGit2Sharp](https://cloud.githubusercontent.com/assets/80104/6771122/f520d570-d092-11e4-9d53-d2430b7bb36c.png)

You can see what each option is by pulling up the help for the command:

![SourceLink.exe index LibGit2Sharp](https://cloud.githubusercontent.com/assets/80104/6771131/5d520c90-d093-11e4-99e7-d3672d0f6e3d.png)

The project file is used to get a list of source files and the pdb file to index. A couple of files are excluded from being indexed since they are modified at build time. The repository root is in the parent directory. SourceLink.exe then verifies that the checksums stored in the pdb file and calculated from the files in the working directory match the checksums stored in the Git repository. That way, when Visual Studio or other debuggers download the source code, the files will match.

## Checksums

The `checksums` command can be used to print a list of all the index source files and their checksums.

![checksums](https://cloud.githubusercontent.com/assets/80104/6773188/87e38ff6-d0ca-11e4-93c5-99d05fc70295.png)

You can print the URLs instead.

![checksums urls](https://cloud.githubusercontent.com/assets/80104/6773232/d9a1fa4e-d0ca-11e4-9f9c-2a8461930cac.png)

And very important, you can check that checksums from downloading the files match what is in the pdb.

![checksums check](https://cloud.githubusercontent.com/assets/80104/6773250/4b0f2e22-d0cb-11e4-9eea-f406abc20278.png)

That covers the options in the help.

![checksums help](https://cloud.githubusercontent.com/assets/80104/6773270/e9b191b4-d0cb-11e4-88bc-be297ee7eeb3.png)
