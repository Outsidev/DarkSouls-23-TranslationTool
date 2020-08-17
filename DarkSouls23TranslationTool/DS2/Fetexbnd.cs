using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool.DS2
{
    class Fetexbnd
    {
        uint bnd = 876891714;//bnd4
        uint idka = 0;
        uint idkb = 65536;
        public uint fileCount;
        ulong idk2 = 64;
        ulong version = 3689103615043188017; //15B21T23
        uint directoryEntrySize = 36;
        uint idk3 = 0;
        long dataOffset = 1802;
        long idk5 = 21504;

        public byte[] bndByteData;
        public string filePath;

        public Fetexbnd()
        {
        }

        public void CreateFetexFromDDSs(string fetexExportedFolderPath)
        {            
            string[] ddsPaths = Directory.GetFiles(fetexExportedFolderPath, "*.dds", SearchOption.AllDirectories);
            TpfFile[] tpfArray = new TpfFile[ddsPaths.Length];
            for (int i = 0; i < ddsPaths.Length; i++)
            {
                TpfFile tpf = new TpfFile();
                string ddsPath = ddsPaths[i].Substring(0, ddsPaths[i].LastIndexOf("\\"));
                string tpfPath = ddsPath+".tpf";
                tpf.Create(ddsPath, tpfPath);
                tpf.fileName = Path.GetFileNameWithoutExtension(ddsPath) + ".tpf";
                tpfArray[i] = tpf;
            }

            string fetexPath = fetexExportedFolderPath+".fetexbnd";            
            fileCount = (uint)tpfArray.Length;
            MemoryStream bnddata = new MemoryStream();
            byte[] orjFetex = File.ReadAllBytes(fetexPath);
            bnddata.Write(orjFetex, 0, 2048);
            using (BinaryWriter binwr = new BinaryWriter(bnddata, Encoding.ASCII))
            {
                //no changes on fist 64 byte header, skipping
                binwr.BaseStream.Position = 64;
                
                int headerPos = (int)binwr.BaseStream.Position;
                int nameOffsets = (int)binwr.BaseStream.Position + 36 * (int)fileCount;
                for (int i = 0; i < fileCount; i++)
                {
                    binwr.Write((int)64);
                    binwr.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                    binwr.Write((long)tpfArray[i].data.Length);
                    binwr.Write((long)tpfArray[i].data.Length);
                    binwr.Write((int)0);//fileoffset, for now 0, coming later
                    binwr.BaseStream.Position += 4;//skip id
                    binwr.Write((int)nameOffsets);//nameoffset
                    nameOffsets += Encoding.ASCII.GetByteCount(tpfArray[i].fileName)+1;
                }

                binwr.BaseStream.Position = 2048;
                headerPos += 24;//fileOffset
                for (int i = 0; i < fileCount; i++)
                {
                    uint tempOffset = (uint)binwr.BaseStream.Position;
                    binwr.Write(tpfArray[i].data);
                    binwr.BaseStream.Position = headerPos + i * 36;
                    binwr.Write(tempOffset);
                    binwr.BaseStream.Position = tempOffset + tpfArray[i].data.Length;
                }

            }

            Console.Write(".");
            bndByteData = bnddata.ToArray();
            filePath = fetexExportedFolderPath + ".fetexbnd";
            //File.WriteAllBytes(fetexExportedFolderPath+".MINECAMP", bnddata.ToArray());
        }
    }
}
