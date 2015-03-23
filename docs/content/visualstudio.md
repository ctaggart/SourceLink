# Enable Source Server Support in Visual Studio

You just need to check the `Enable source server support` in Debugging > Options and set the path for `Cache symbols in this directory`. The path is used to save source files too, not just pdb (symbol) files. They are cached in a subdirectory named `src`. No symbol (pdb) servers need to be enabled if the pdb files are distributed with the NuGet packages.

![Enable source server support](http://2.bp.blogspot.com/-v8A1ZnHodZI/U7y3o8gzuSI/AAAAAAAAOWg/LmW_k2KAk7E/s1600/Options.png)

![Cache symbols in this directory](http://1.bp.blogspot.com/-7LzFgBIPupc/U7y3xxow5qI/AAAAAAAAOWo/QD2cXv68p8I/s1600/Symbols.png)