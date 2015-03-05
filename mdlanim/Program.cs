using System;
using System.Collections.Generic;
using System.Text;

using BrawlLib.Modeling;
using System.Linq;
using BrawlLib.SSBB.ResourceNodes;
using System.IO;


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
            var pac   = args.FirstOrDefault(f => f.EndsWith(".pac"));


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
            else if(pac != null) {
                ((ARCNode)NodeFactory.FromFile(null, pac)).ExtractToFolder(Path.GetFileNameWithoutExtension(pac) + ".extracted");

            } 
            else
            {
                Console.Error.WriteLine("No mdl0 specified!");
            }
        }
    }
}
