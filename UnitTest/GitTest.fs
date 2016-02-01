module SourceLink.GitTest

open Xunit
open FsUnit.Xunit

[<Fact>]
let ``dummy test`` () =
    "test" |> should equal "test"

// TODO tests about origin
// remote origin https://github.com/ctaggart/SourceLink.git