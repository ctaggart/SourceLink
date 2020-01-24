using System.Reflection;

namespace SourceLink
{
    public static class Version
    {
        public static string GetAssemblyInformationalVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute == null) return null;
            return attribute.InformationalVersion;
        }
    }
}
