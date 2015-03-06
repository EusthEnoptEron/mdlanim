using System;
using System.Collections.Generic;
using System.Text;

using BrawlLib.Modeling;
using System.Linq;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;
using BrawlLib.IO;


namespace mdlanim
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1 && Directory.Exists(args[0]))
            {
                args = Directory.GetFiles(args[0]);
            }
            var mdl0path = args.FirstOrDefault(f => f.EndsWith(".mdl0"));
            var anims = args.Where(f => f.EndsWith(".chr0"));
            var textures = args.Where(f => f.EndsWith(".tex0"));
            var pacList   = args.Where(f => f.EndsWith(".pac"));
            var brresList = args.Where(f => f.EndsWith(".brres") || f.EndsWith(".kyp") || f.EndsWith(".mca"));

            // Convert textures
            foreach (var tex in textures.Select(t => ((TEX0Node)NodeFactory.FromFile(null, t))))
            {
                tex.Export(Path.GetFileNameWithoutExtension(tex.FilePath) + ".png");
            }

            if (mdl0path != null)
            {
                var outputPath = Path.GetFileNameWithoutExtension(mdl0path) + ".dae";
                var model = MDL0Node.FromFile(mdl0path);
                var animations = anims.Select(anim => CHR0Node.FromFile(anim));

                // First write the DAE file
                Collada.Serialize(model, outputPath);

                // Write animations    
                Collada.Serialize(model, animations.ToArray(), 60f, outputPath);
            }
            else if(pacList.Count() > 0) {
                foreach (var pac in pacList)
                {
                    var node = ((ARCNode)NodeFactory.FromFile(null, pac));

                    node.ExtractToFolder(pac + ".extracted");
                }

            }
            else if (brresList.Count() > 0)
            {
                foreach (var brres in brresList)
                {
                    Console.WriteLine("Extracting {0}", Path.GetFileName(brres));
                    try
                    {
                        if (brres.EndsWith(".kyp"))
                        {
                            using(var fileMap = FileMap.FromFile(brres))
                            {
                                // Skip header
                                var ds = new DataSource(fileMap);
                                var startAddress = ds.Address;

                                ds.Address += 0x18;
                                StringBuilder sb = new StringBuilder();
                                while (ds.Address.Byte != 0)
                                {
                                    sb.Append((char)ds.Address.Byte);
                                    ds.Address++;
                                }
                                
                                ds.Address = startAddress + 0x60;

                                string path = Path.Combine( Path.GetDirectoryName(brres), sb.ToString());
                                Console.WriteLine("   {0} -> {1}", Path.GetFileName(brres), sb.ToString());

                                ((BRRESNode)NodeFactory.FromSource(null, ds)).ExportToFolder(path, ".png");
                            }

                        }
                        else if (brres.EndsWith(".mca"))
                        {
                            using (var fileMap = FileMap.FromFile(brres))
                            {
                                // Skip header
                                var ds = new DataSource(fileMap);
                                //var startAddress = ds.Address;

                                // bres
                                while(ds.Address.Int != ((0x62 << 24) | (0x72 << 16) | (0x65 << 8) | 0x73))
                                //while (!(ds.Address.Byte == 0x62 && (ds.Address + 1).Byte == 0x72 && (ds.Address + 2).Byte == 0x65 && (ds.Address + 3).Byte == 0x73))
                                {
                                    ds.Address++;
                                }

                                //Console.Write("FOUND at " +((int)(ds.Address - startAddress)));

                                ((BRRESNode)NodeFactory.FromSource(null, ds)).ExportToFolder(brres + ".extracted", ".png");
                            }
                        }
                        else
                            ((BRRESNode)NodeFactory.FromFile(null, brres)).ExportToFolder(brres + ".extracted", ".png");
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }

                }
            } else
            {
                Console.Error.WriteLine("No mdl0 specified!");
            }
        }
    }
}
