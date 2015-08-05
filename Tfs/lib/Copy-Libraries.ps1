# copy libraries from Team Explorer
$ext = gp 'HKCU:\SOFTWARE\Microsoft\VisualStudio\14.0\ExtensionManager\EnabledExtensions\'
$key = $ext | Get-Member | select -ExpandProperty Name | where {$_ -like 'Microsoft.VisualStudio.TeamFoundation.TeamExplorer.Extensions,14*'}
$from = $ext.($key)
copy $from\Microsoft.TeamFoundation.Build.Activities.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Build.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Build.Common.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Build.Workflow.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Common.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.Git.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.SourceControl.WebApi.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.TestManagement.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.VersionControl.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.VersionControl.Common.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.WorkItemTracking.Client.dll $PSScriptRoot
copy $from\Microsoft.TeamFoundation.WorkItemTracking.Common.dll $PSScriptRoot
copy $from\Microsoft.VisualStudio.Services.Common.dll $PSScriptRoot
copy $from\Microsoft.VisualStudio.Services.WebApi.dll $PSScriptRoot