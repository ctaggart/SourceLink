# Line Endings
The checksums of the files that the compiler uses are stored in the `pdb`. Unfortunately, Git on Windows behaves like `core.autocrlf=true` by default. It will auto convert the line endings to `crlf` (carriage return, line feed) instead of leaving them just `lf` (line feed). They are stored in the Git repository as `lf` if you used a `.gitattributes` file which is recommended. Here are a some different ways to keep Git on Windows from auto converting line endings.

  * configure git to use core.autocrlf input globally *before* cloning

    `git config --global core.autocrlf input`

    often done with AppVeyor in `init` section

    examples: [octokit.net](https://github.com/octokit/octokit.net/blob/master/appveyor.yml#L2), [FSharp.Data](https://github.com/fsharp/FSharp.Data/blob/master/appveyor.yml#L2)

  * configure git to use core.autocrlf input *when* cloning

    `git clone https://github.com/octokit/octokit.net.git -c core.autocrlf=input`

  * configure the source files to default to `lf` in `.gitattributes`

    `*.cs eol=lf`

    `*.fs eol=lf`

    examples: [FSharp.Compiler.Service](https://github.com/fsharp/FSharp.Compiler.Service/blob/master/.gitattributes#L5), [SourceLink](https://github.com/ctaggart/SourceLink/blob/master/.gitattributes#L5)

    This just changes the default behavior. It can still be overridden using `core.autocrlf=true` if someone really wants `crlf`.
