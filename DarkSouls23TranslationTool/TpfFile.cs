using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class DdsFile
    {
        uint dataOffset;
        public uint dataSize;
        int settings;
        int fileNameOffset;
        int idk;//0?

        string fileName;
        public byte[] data;

        public DdsFile()
        {
        }

        public DdsFile(string ddsPath)
        {
            data = File.ReadAllBytes(ddsPath);
            dataSize = (uint)data.Length;

        }

        public void FillData(BinaryReader binred)
        {
            dataOffset = binred.ReadUInt32();
            dataSize = binred.ReadUInt32();
            settings = binred.ReadInt32();
            fileNameOffset = binred.ReadInt32();
            idk = binred.ReadInt32();

            long holdpos = binred.BaseStream.Position;
            binred.BaseStream.Position = fileNameOffset;
            fileName = Tools.ReadCharTillNull(binred);
            binred.BaseStream.Position = dataOffset;
            data = binred.ReadBytes((int)dataSize);

            binred.BaseStream.Position = holdpos;
        }        
    }


    class TpfFile : BndEntry
    {

        int header = 4608084;
        uint ddsSumSize;
        uint entryCount;
        uint idk = 66304;

        public TpfFile()
        {
        }


        //idk, be continued?
        public void Export(string filePath)
        {
            using (BinaryReader binred = new BinaryReader(File.Open(filePath, FileMode.Open),Encoding.Unicode))
            {
                int header = binred.ReadInt32();
                uint size = binred.ReadUInt32();
                uint entryCount = binred.ReadUInt32();

                int idk = binred.ReadInt32();

                DdsFile[] ddsFileArray = new DdsFile[entryCount];
                for (int i = 0; i < entryCount; i++)
                {
                    DdsFile dds = new DdsFile();
                    dds.FillData(binred);
                }
            }
        }

        public void Create(string ddsPath, string tpfPath)
        {
            string[] ddsPaths = Directory.GetFiles(ddsPath, "*.dds");
            DdsFile[] ddsFiles = new DdsFile[ddsPaths.Length];
            for (int i = 0; i < ddsPaths.Length; i++)
            {
                ddsFiles[i] = new DdsFile(ddsPaths[i]);
            }

            ddsSumSize = (uint)ddsFiles.Sum(p=>p.dataSize);
            entryCount = (uint)ddsFiles.Length;
            MemoryStream memData = new MemoryStream();
            using(BinaryReader binred = new BinaryReader(new MemoryStream(File.ReadAllBytes(tpfPath))))
            {
                using (BinaryWriter binwr = new BinaryWriter(memData))
                {
                    binwr.Write(binred.ReadInt32());
                    binwr.Write(ddsSumSize);
                    binwr.Write(entryCount);
                    binred.BaseStream.Position = 12; 
                    binwr.Write(binred.ReadInt32());//idk

                    uint holdDataStartPos = binred.ReadUInt32();
                    uint dataStartPos = holdDataStartPos;
                    binred.BaseStream.Position += 4;
                    for (int i = 0; i < entryCount; i++)
                    {
                        binwr.Write(dataStartPos);
                        binwr.Write(ddsFiles[i].dataSize);
                        binwr.Write(binred.ReadInt32());//idk
                        binwr.Write(binred.ReadInt32());//fileNameOffset
                        binwr.Write(binred.ReadInt32());//0
                        dataStartPos += ddsFiles[i].dataSize;
                        binred.BaseStream.Position += 8;
                    }
                    binred.BaseStream.Position -= 8;
                    int fileNameBytesLen = (int)holdDataStartPos - (int)binwr.BaseStream.Position;
                    binwr.Write(binred.ReadBytes(fileNameBytesLen));
                    for (int i = 0; i < entryCount; i++)
                    {
                        binwr.Write(ddsFiles[i].data);
                    }
                }
            }

            data = memData.ToArray();
        }

        public void CreateFromExistingTpf(string tpfPath)
        {
            data = File.ReadAllBytes(tpfPath);
            fileName = Path.GetFileName(tpfPath);
        }
    }
}
