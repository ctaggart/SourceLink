(Get-Item Env:\PSModulePath).Value -Split ';'
Paket-Restore
.\packages\FAKE\tools\FAKE.exe build.fsx @args