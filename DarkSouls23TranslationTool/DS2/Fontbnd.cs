using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool.DS2
{
    class Fontbnd
    {
        uint bnd = 876891714;//bnd4
        uint idka = 0;
        uint idkb = 65536;
        public uint fileCount;
        ulong idk2 = 64;
        ulong version = 16131176436413489; //14M18O9.
        uint directoryEntrySize = 36;
        uint idk3 = 0;
        long dataOffset = 1802;
        long idk5 = 21504;

        public byte[] bndByteData;
        public string filePath;

        public Fontbnd()
        {
        }

        public void CreateFontFromFiles(string fontExportedFolderPath)
        {

            string ccmXmlPath   = Path.Combine(fontExportedFolderPath, Path.GetFileName(fontExportedFolderPath + ".ccm.xml"));
            string ccmPath      = Path.Combine(fontExportedFolderPath, Path.GetFileName(fontExportedFolderPath + ".ccm"));
            CcmFile ccmFile;
            if (File.Exists(ccmXmlPath))
            {
                ccmFile = new CcmFile();
                ccmFile.CreateCcm(ccmPath, ccmXmlPath);
            }                
            else
                ccmFile = new CcmFile(ccmPath);           

            string[] ddsPaths = Directory.GetFiles(fontExportedFolderPath, "*.dds", SearchOption.AllDirectories);
            TpfFile[] tpfArray = new TpfFile[ddsPaths.Length];

            for (int i = 0; i < ddsPaths.Length; i++)
            {
                TpfFile tpf = new TpfFile();
                string ddsPath = Path.GetDirectoryName(ddsPaths[i]);
                string tpfPath = ddsPath + ".tpf";
                tpf.Create(ddsPath, tpfPath);
                tpf.fileName = Path.GetFileNameWithoutExtension(ddsPath) + ".tpf";
                tpfArray[i] = tpf;
            }

            List<BndEntry> bndEntries = new List<BndEntry>();
            bndEntries.Add(ccmFile);
            bndEntries.AddRange(tpfArray);
            
            string orjBndPath = fontExportedFolderPath + ".fontbnd";
            fileCount = (uint)bndEntries.Count;
            MemoryStream newBndData = new MemoryStream();
            int firstFileOffset = 0;
            using (BinaryReader binred = new BinaryReader(File.Open(orjBndPath, FileMode.Open, FileAccess.Read)))
            {   //Copy orginal bnd file's first 1024 byte //why 1024?: first file starts at 1024
                binred.BaseStream.Position = 88;
                firstFileOffset = binred.ReadInt32();
                binred.BaseStream.Position = 0;
                newBndData.Write(binred.ReadBytes(firstFileOffset), 0, firstFileOffset);
            }

            using (BinaryWriter binwr = new BinaryWriter(newBndData, Encoding.ASCII))
            {
                //no changes on first 64 byte header, skipping
                binwr.BaseStream.Position = 64;

                int headerPos = (int)binwr.BaseStream.Position;
                int nameOffsets = (int)binwr.BaseStream.Position + 36 * (int)fileCount;

                for (int i = 0; i < fileCount; i++)
                {
                    binwr.Write((int)64);
                    binwr.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                    binwr.Write((long)bndEntries[i].data.Length);
                    binwr.Write((long)bndEntries[i].data.Length);
                    binwr.Write((int)0);//fileoffset, for now 0, coming later
                    binwr.BaseStream.Position += 4;//skip id
                    binwr.Write((int)nameOffsets);//nameoffset
                    nameOffsets += Encoding.ASCII.GetByteCount(bndEntries[i].fileName) + 1;
                }

                binwr.BaseStream.Position = firstFileOffset;
                headerPos += 24;//fileOffset
                for (int i = 0; i < fileCount; i++)
                {
                    uint tempOffset = (uint)binwr.BaseStream.Position;
                    binwr.Write(bndEntries[i].data);
                    binwr.BaseStream.Position = headerPos + i * 36;
                    binwr.Write(tempOffset);
                    binwr.BaseStream.Position = tempOffset + bndEntries[i].data.Length;
                }

            }

            Console.Write(".");
            bndByteData = newBndData.ToArray();
            filePath = fontExportedFolderPath + ".fontbnd";
            //File.WriteAllBytes(filePath+".MINECAMP", bndByteData.ToArray());
        }

    }
}
