[<AutoOpen>]
module SourceLink.AppConfigM

open System.Configuration
open System.IO

type AppConfig = System.Configuration.Configuration

type Configuration with
    static member Load (path:string) =
        let map = ExeConfigurationFileMap()
        map.ExeConfigFilename <- path
        ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None) // preLoad=true param not available on Mono 3.2.5
    static member Create (path:string) =
        do 
            use sw = new StreamWriter(path)
//            sw.WriteLine "<?xml version=\"1.0\" encoding=\"utf-8\"?>" // not needed
            sw.WriteLine "<configuration>"
            sw.WriteLine "  <appSettings />" // 2 spaces
            sw.WriteLine "</configuration>"
        Configuration.Load path
    /// loads or creates the config file
    static member Get path =
        if File.Exists path then Configuration.Load path
        else Configuration.Create path
        
    member x.SaveModified() = x.Save ConfigurationSaveMode.Modified

type AppSettingsSection with
    member x.Item
        with get k =
            let kv = x.Settings.Item k
            if kv = null then None
            else kv.Value |> Some
        and set k (v:option<string>) =
            if v.IsSome then
                if x.Settings.Item k <> null then
                    x.Settings.Remove k
                x.Settings.Add(k, v.Value)
            else x.Settings.Remove k