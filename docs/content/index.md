# SourceLink: Source Code On Demand

SourceLink allows library users to download on demand the exact version of the source files that were used to build the library or application while debugging from Visual Studio or another Windows debugger. The debuggers use [srcsrv.dll](http://msdn.microsoft.com/en-us/library/windows/hardware/ff558791.aspx) to retrieve the source code. "Because the source code for a module can change between versions and over the course of years, it is important to look at the source code as it existed when the version of the module in question was built." SourceLink enables the source control management (SCM) system to be the [source server](http://msdn.microsoft.com/en-us/library/windows/desktop/ms680641.aspx) by indexing the `pdb` files with `https` links to the SCM. Access to the source code is controlled by the SCM.

## For Library Authors
Install [SourceLink.exe](https://chocolatey.org/packages/SourceLink) via chocolatey.

```
choco install SourceLink
```

If you use FAKE for .NET builds, [SourceLink.Fake](fake.html) can be used. If you use TFS, you can use SourceLink.exe or [SourceLink.Tfs](tfs.html) which allows FAKE to be used from TFS 2013 and Visual Studio Online.

![](https://cloud.githubusercontent.com/assets/80104/8490526/75457d5e-20df-11e5-90db-1e7da20e1991.png)

You can see a list of available commands simply by running `SourceLink.exe`.

![](https://cloud.githubusercontent.com/assets/80104/8490543/c9c598a0-20df-11e5-997a-bafc4dc54499.png)

The `index` command is the one that will create the index and put it in the pdb file.

![](https://cloud.githubusercontent.com/assets/80104/8490561/f873cee2-20df-11e5-95ee-b64d96418c93.png)

An example is [source indexing FSharp.Core.pdb](https://github.com/Microsoft/visualfsharp/issues/294#issuecomment-117922233):

```
SourceLink.exe index `
-pr ./src/fsharp/FSharp.Core/FSharp.Core.fsproj `
-pp Configuration Release `
-u 'https://raw.githubusercontent.com/Microsoft/visualfsharp/{0}/%var2%'
```

The pdb files can be distributed with the NuGet library, with your web app, app, or put in a symbol server. Ideally, all open source NuGet libraries in the NuGet Gallery would contain source indexed pdb files.

## For Library Users

You need to [enable source server support in Visual Studio](visualstudio.html) or other app.

### Demo: Source Code Downloads when Debugging

When the libraries are source indexed, you can step directly into the source code. It is downloaded on demand.

<iframe width="700" height="450" src="https://www.youtube.com/embed/k_jeSP_rMp8?rel=0" frameborder="0" allowfullscreen></iframe>

### Demo: Go To Source during Design Time

Visual F# Power Tools 1.8 added [Navigate to Source](http://fsprojects.github.io/VisualFSharpPowerTools/navigatetosource.html) so that you can hit F12 and view the source code during design time! It uses source indexed pdb files with [SourceLink.SymbolStore](https://www.nuget.org/packages/SourceLink.SymbolStore/).

[![](https://cloud.githubusercontent.com/assets/80104/8491836/cb10f2da-20f4-11e5-8def-b53eb5b1a3c8.png)](https://twitter.com/ploeh/status/614328824969490432)

<iframe title="YouTube video player" width="700" height="450" src="https://www.youtube.com/embed/5n3TUqMiysk?rel=0" frameborder="0" allowfullscreen></iframe>
