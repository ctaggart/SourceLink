REM Why is not there a "nuget install SourceLink.sln" when there is a "nuget update SourceLink.sln"?
nuget install ..\Build\packages.config -o ..\packages
nuget install ..\ConsoleTest\packages.config -o ..\packages
nuget install ..\Pe\packages.config -o ..\packages