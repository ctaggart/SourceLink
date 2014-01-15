namespace SourceLink

open System
open System.Collections.Generic
open Microsoft.TeamFoundation.Build.Workflow.Activities
open Microsoft.TeamFoundation.Build.Workflow
open Microsoft.TeamFoundation.Build.Client
open SourceLink

type TfsProcessParameters(prms:(IDictionary<string,obj>)) =
    new() = TfsProcessParameters(Dictionary<string,obj>())
    member x.Dictionary with get() = prms
    member x.Add k v = prms.Add(k,v)
    member x.AddString k (v:string) = x.Add k (box v)
    member x.AddInt k (v:int) = x.Add k (box v)
    member x.AddBool k (v:bool) = x.Add k (box v)
    member x.Get k = prms.Get k
    member x.Set k v = prms.Set k v
    member x.Remove (k:string) = prms.Remove k |> ignore
    member x.Item
        with get k = prms.[k]
        and set k (v:obj) = prms.[k] <- v

    // TFVC and Git
    member x.BuildNumberFormat 
        with get() = match x.Get "BuildNumberFormat" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["BuildNumberFormat"] <- v | None -> x.Remove "BuildNumberFormat"
    member x.AgentSettings 
        with get() = match x.Get "AgentSettings" with | Some o -> o :?> AgentSettings |> Some | None -> None
        and set (value:option<AgentSettings>) = match value with | Some v -> x.["AgentSettings"] <- v | None -> x.Remove "AgentSettings"
    member x.MSBuildArguments 
        with get() = match x.Get "MSBuildArguments" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["MSBuildArguments"] <- v | None -> x.Remove "MSBuildArguments"
    member x.MSBuildPlatform 
        with get() = match x.Get "MSBuildPlatform" with | Some o -> o :?> ToolPlatform |> Some | None -> None
        and set (value:option<ToolPlatform>) = match value with | Some v -> x.["MSBuildPlatform"] <- v | None -> x.Remove "MSBuildPlatform"
    member x.MSBuildMultiProc 
        with get() = match x.Get "MSBuildMultiProc" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["MSBuildMultiProc"] <- v | None -> x.Remove "MSBuildMultiProc"
    member x.Verbosity 
        with get() = match x.Get "Verbosity" with | Some o -> o :?> BuildVerbosity |> Some | None -> None
        and set (value:option<BuildVerbosity>) = match value with | Some v -> x.["Verbosity"] <- v | None -> x.Remove "Verbosity"
    member x.Metadata 
        with get() = match x.Get "Metadata" with | Some o -> o :?> ProcessParameterMetadataCollection |> Some | None -> None
        and set (value:option<ProcessParameterMetadataCollection>) = match value with | Some v -> x.["Metadata"] <- v | None -> x.Remove "Metadata"
    member x.SupportedReasons 
        with get() = match x.Get "SupportedReasons" with | Some o -> o :?> BuildReason |> Some | None -> None
        and set (value:option<BuildReason>) = match value with | Some v -> x.["SupportedReasons"] <- v | None -> x.Remove "SupportedReasons"
    member x.BuildProcessVersion 
        with get() = match x.Get "BuildProcessVersion" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["BuildProcessVersion"] <- v | None -> x.Remove "BuildProcessVersion"

    // TFVC
    member x.BuildSettings 
        with get() = match x.Get "BuildSettings" with | Some o -> o :?> BuildSettings |> Some | None -> None
        and set (value:option<BuildSettings>) = match value with | Some v -> x.["BuildSettings"] <- v | None -> x.Remove "BuildSettings"
    member x.ProjectsToBuild
        with get() =
            if x.BuildSettings.IsNone then None 
            else if x.BuildSettings.Value.HasProjectsToBuild = false then None
            else
                let projects = x.BuildSettings.Value.ProjectsToBuild.ToArray()
                if projects.Length = 0 then None else Some projects
    member x.PlatformConfigurations
        with get() =
            if x.BuildSettings.IsNone then None 
            else if x.BuildSettings.Value.HasPlatformConfigurations = false then None
            else
                let configurations = x.BuildSettings.Value.PlatformConfigurations.ToArray()
                if configurations.Length = 0 then None else Some configurations
        and set (pcs:option<PlatformConfiguration[]>) =
            if pcs.IsNone then
                if x.BuildSettings.IsSome then
                    x.BuildSettings.Value.PlatformConfigurations.Clear()
            else
                if x.BuildSettings.IsNone then x.BuildSettings <- BuildSettings() |> Some
                x.BuildSettings.Value.PlatformConfigurations <- PlatformConfigurationList pcs.Value
    /// Some if only one project, else None
    member x.ProjectToBuild
        with get() =
            let ps = x.ProjectsToBuild
            if ps.IsNone then None
            else if ps.Value.Length = 1 then Some ps.Value.[0]
            else None
    /// Some if only one platform configuration, else None
    member x.PlatformConfiguration
        with get() =
            let pcs = x.PlatformConfigurations
            if pcs.IsNone then None
            else if pcs.Value.Length = 1 then Some pcs.Value.[0]
            else None
        and set (pc:option<PlatformConfiguration>) =
            if pc.IsNone then x.PlatformConfigurations <- None
            else x.PlatformConfigurations <- Some [|pc.Value|]
    member x.TestSpecs 
        with get() = match x.Get "TestSpecs" with | Some o -> o :?> TestSpecList |> Some | None -> None
        and set (value:option<TestSpecList>) = match value with | Some v -> x.["TestSpecs"] <- v | None -> x.Remove "TestSpecs"
    member x.CleanWorkspace 
        with get() = match x.Get "CleanWorkspace" with | Some o -> o :?> CleanWorkspaceOption |> Some | None -> None
        and set (value:option<CleanWorkspaceOption>) = match value with | Some v -> x.["CleanWorkspace"] <- v | None -> x.Remove "CleanWorkspace"
    member x.RunCodeAnalysis 
        with get() = match x.Get "RunCodeAnalysis" with | Some o -> o :?> CodeAnalysisOption |> Some | None -> None
        and set (value:option<CodeAnalysisOption>) = match value with | Some v -> x.["RunCodeAnalysis"] <- v | None -> x.Remove "RunCodeAnalysis"
    member x.SourceAndSymbolServerSettings 
        with get() = match x.Get "SourceAndSymbolServerSettings" with | Some o -> o :?> SourceAndSymbolServerSettings |> Some | None -> None
        and set (value:option<SourceAndSymbolServerSettings>) = match value with | Some v -> x.["SourceAndSymbolServerSettings"] <- v | None -> x.Remove "SourceAndSymbolServerSettings"
    member x.CreateWorkItem 
        with get() = match x.Get "CreateWorkItem" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CreateWorkItem"] <- v | None -> x.Remove "CreateWorkItem"
    member x.PerformTestImpactAnalysis 
        with get() = match x.Get "PerformTestImpactAnalysis" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["PerformTestImpactAnalysis"] <- v | None -> x.Remove "PerformTestImpactAnalysis"
    member x.CreateLabel 
        with get() = match x.Get "CreateLabel" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CreateLabel"] <- v | None -> x.Remove "CreateLabel"
    member x.DisableTests 
        with get() = match x.Get "DisableTests" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["DisableTests"] <- v | None -> x.Remove "DisableTests"

    // Git
    member x.SolutionToBuild 
        with get() = match x.Get "SolutionToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["SolutionToBuild"] <- v | None -> x.Remove "SolutionToBuild"
    member x.ConfigurationToBuild 
        with get() = match x.Get "ConfigurationToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["ConfigurationToBuild"] <- v | None -> x.Remove "ConfigurationToBuild"
    member x.PlatformToBuild 
        with get() = match x.Get "PlatformToBuild" with | Some o -> o :?> string |> Some | None -> None
        and set (value:option<string>) = match value with | Some v -> x.["PlatformToBuild"] <- v | None -> x.Remove "PlatformToBuild"
    member x.TestSpec 
        with get() = match x.Get "TestSpec" with | Some o -> o :?> AgileTestPlatformSpec |> Some | None -> None
        and set (value:option<AgileTestPlatformSpec>) = match value with | Some v -> x.["TestSpec"] <- v | None -> x.Remove "TestSpec"
    member x.CleanRepository 
        with get() = match x.Get "CleanRepository" with | Some o -> o :?> bool |> Some | None -> None
        and set (value:option<bool>) = match value with | Some v -> x.["CleanRepository"] <- v | None -> x.Remove "CleanRepository"

