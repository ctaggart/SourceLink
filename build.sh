#!/bin/bash
if [ ! -f packages/FAKE/tools/Fake.exe ]; then
  mono .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
if [ ! -f packages/SourceLink.Fake/tools/Fake.fsx ]; then
  mono .nuget/NuGet.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion -Prerelease
fi
mono packages/FAKE/tools/FAKE.exe build.fsx -d:MONO $@
