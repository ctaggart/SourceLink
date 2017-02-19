using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        var app = new CommandLineApplication()
        {
            Name = "dotnet sourcelink",
            FullName = "SourceLink: Source Code On Demand",
            Description = "Source Link your Portable PDB files to allow source code to be downloaded on demand from the source code repository host"
        };
        app.HelpOption("-h|--help");

        app.Command("print-json", command =>
        {
            command.Description = "print the Source Link JSON stored in the Portable PDB file";
            var pdbOption = command.Option("-p|--pdb <PDB>", "set path to Porable PDB", CommandOptionType.SingleValue);
            command.HelpOption("-h|--help");

            command.OnExecute(() =>
            {
                if (!pdbOption.HasValue())
                {
                    command.ShowHelp();
                    return 2;
                }
                var path = pdbOption.Value();
                if (!File.Exists(path))
                {
                    Console.WriteLine("PDB file does not exist");
                    return 3;
                }
                var bytes = GetSourceLinkBytes(path);
                if(bytes.Length == 0)
                {
                    Console.WriteLine("Source Link JSON not found in PDB file");
                    return 4;
                }
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
                return 0;
            });
        });

        if (args.Length == 0)
        {
            app.ShowHelp();
            return 0;
        }
        app.Execute(args);
        return 0;
    }

    static readonly Guid SourceLinkId = new Guid("CC110556-A091-4D38-9FEC-25AB9A351A6A");

    static byte[] GetSourceLinkBytes(string pdb)
    {
        using (var mrp = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdb)))
        {
            var mr = mrp.GetMetadataReader();
            var blobh = default(BlobHandle);
            foreach (var cdih in mr.GetCustomDebugInformation(EntityHandle.ModuleDefinition))
            {
                var cdi = mr.GetCustomDebugInformation(cdih);
                if (mr.GetGuid(cdi.Kind) == SourceLinkId)
                {
                    blobh = cdi.Value;
                }
            }
            if (blobh.IsNil) return Array.Empty<byte>();
            return mr.GetBlobBytes(blobh);
        }
    }
}