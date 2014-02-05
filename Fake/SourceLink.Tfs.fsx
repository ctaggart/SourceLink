namespace SourceLink

#load "SourceLink.fsx"
#if MONO
#else
#load "SourceLink.Tfs.Assemblies.fsx"
#endif

open Fake
open SourceLink

[<AutoOpen>]
module TfsM =

    let isTfsBuild = hasBuildParam "tfsBuild"

    #if MONO
    #else
    let getTfsBuild() =
        if isTfsBuild then new TfsBuild(getBuildParam "tfsUri", getBuildParam "tfsUser", getBuildParam "tfsAgent", getBuildParam "tfsBuild")
        else Ex.failwithf "isTfsBuild = false"
    #endif