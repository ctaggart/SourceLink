using System;

namespace SourceLink
{
    // https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md#document-table-0x30
    public class Document
    {
        public string Name { get; set; }
        public Guid HashAlgorithm { get; set; }
        public byte[] Hash { get; set; }
        public Guid Language { get; set; }

        public string Url { get; set; }
    }

    public static class HashAlgorithmGuids
    {
        static readonly Guid md5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
        static readonly Guid sha1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");
        static readonly Guid sha256 = new Guid("8829d00f-11b8-4213-878b-770e8597ac16");

        public static string GetName(Guid guid)
        {
            if (guid == md5) return "md5";
            if (guid == sha1) return "sha1";
            if (guid == sha256) return "sha256";
            return guid.ToString();
        }
    }

    // https://github.com/jbevain/cecil/blob/master/Mono.Cecil.Cil/PortablePdb.cs

    public static class LanguageGuids
    {
        static readonly Guid csharp = new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1");
        static readonly Guid fsharp = new Guid("ab4f38c9-b6e6-43ba-be3b-58080b2ccce3");
        static readonly Guid basic = new Guid("3a12d0b8-c26c-11d0-b442-00a0244a1dd2");

        public static string GetName(Guid guid)
        {
            if (guid == csharp) return "csharp";
            if (guid == fsharp) return "fsharp";
            if (guid == basic) return "basic";
            return guid.ToString();
        }
    }
}
