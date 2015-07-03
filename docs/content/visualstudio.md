# Enable Source Server Support in Visual Studio

You just need to check the `Enable source server support` in Debugging > Options.

![Enable source server support](http://2.bp.blogspot.com/-v8A1ZnHodZI/U7y3o8gzuSI/AAAAAAAAOWg/LmW_k2KAk7E/s1600/Options.png)

You also need to set the path for `Cache symbols in this directory`. The path is used to save source files too, not just pdb (symbol) files. They are cached in a subdirectory named `src`. No symbol (pdb) servers need to be enabled if the pdb files are distributed with the NuGet packages.

![Cache symbols in this directory](http://1.bp.blogspot.com/-7LzFgBIPupc/U7y3xxow5qI/AAAAAAAAOWo/QD2cXv68p8I/s1600/Symbols.png)

While debugging, if you get a message like this:

    The debug source files settings for the active solution indicate that the debugger will not ask the user to find the file:

it means you clicked cancel once before you it prompted you to find the source file. You need to clear this list of `Do not look at these source files` in the solution properties before trying again.
![](https://cloud.githubusercontent.com/assets/80104/8489262/8a8fb33e-20ce-11e5-811a-67b94538664e.png)
