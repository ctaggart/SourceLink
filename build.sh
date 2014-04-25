#!/bin/bash
mono .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
mono .nuget/NuGet.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion -Prerelease
mono packages/FAKE/tools/FAKE.exe build.fsx -d:MONO $@
