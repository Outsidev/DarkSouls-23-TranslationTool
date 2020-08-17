using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class FmgFile
    {
        public string fileName;
        public List<FmgIdRange> idRangesList;
        public List<FmgString> linesList;
        public uint fileId;
        public byte[] fmgByteData;

        long[] stringOffsetsArray;
        long startPos;

        uint idk = 131072;
        public uint fileSize;
        uint idk2 = 1;
        uint idRangeCount;
        uint stringOffsetCount;
        uint idk3 = 255;
        long stringOffsetsOffset;
        uint idk4 = 0;
        uint idk5 = 0;

        public FmgFile()
        {
            linesList = new List<FmgString>();
            idRangesList = new List<FmgIdRange>();
        }

        public void FillData(BinaryReader binred, uint dataOffset, int dataSize, uint nameOffset, uint fileId)
        {            
            binred.BaseStream.Position = nameOffset;
            fileName = Tools.ReadCharTillNull(binred);
            binred.BaseStream.Position = dataOffset;
            this.fileId = fileId;

            startPos = binred.BaseStream.Position;

            idk = binred.ReadUInt32();
            fileSize = binred.ReadUInt32();
            idk2 = binred.ReadUInt32();
            idRangeCount = binred.ReadUInt32();
            stringOffsetCount = binred.ReadUInt32();
            idk3 = binred.ReadUInt32();
            stringOffsetsOffset = binred.ReadInt64();
            idk4 = binred.ReadUInt32();
            idk5 = binred.ReadUInt32();

            idRangesList = new List<FmgIdRange>();
            for (int i = 0; i < idRangeCount; i++)
            {
                FmgIdRange fid = new FmgIdRange();
                fid.offsetIndex = binred.ReadUInt32();
                fid.firstId = binred.ReadUInt32();
                fid.lastId = binred.ReadUInt32();
                fid.idk = binred.ReadUInt32();
                fid.idCount = fid.lastId - fid.firstId + 1;
                idRangesList.Add(fid);
            }
            
            binred.BaseStream.Position = startPos + stringOffsetsOffset;
            stringOffsetsArray = new long[stringOffsetCount];
            for (int i = 0; i < stringOffsetCount; i++)
            {
                stringOffsetsArray[i] = binred.ReadInt64();
            }

            linesList = new List<FmgString>();
            foreach(FmgIdRange idRange in idRangesList)
            {
                for (int i = 0; i < idRange.idCount; i++)
                {
                    binred.BaseStream.Position = startPos + stringOffsetsArray[idRange.offsetIndex + i];
                    FmgString line = new FmgString();
                    line.id = (uint)(idRange.firstId + i);
                    line.str = Tools.ReadCharTillNull(binred);
                    line.firstorLast = 0;
                    if (i == 0)
                        line.firstorLast = 1;
                    else if (i == idRange.idCount - 1)
                        line.firstorLast = -1;
                    linesList.Add(line);
                }
            }           
        }

        public void WriteByteData()
        {

            fileSize = 0;
            idRangeCount = (uint)idRangesList.Count;
            stringOffsetCount = (uint)linesList.Count;

            int headerSize = 40;
            int idRangeSize = (int)(idRangeCount * 16);
            stringOffsetsOffset = headerSize + idRangeSize;

            MemoryStream data = new MemoryStream();
            using (BinaryWriter binwr = new BinaryWriter(data, Encoding.Unicode))
            {
                
                for (int i = 0; i < 10; i++)
                    binwr.Write((uint)0);                
                                
                for (int i = 0; i < idRangeCount; i++)
                {
                    FmgIdRange fid = idRangesList[i];
                    binwr.Write(fid.offsetIndex);
                    binwr.Write(fid.firstId);
                    binwr.Write(fid.lastId);
                    binwr.Write(fid.idk);                    
                }

                long strOffsets = (int)binwr.BaseStream.Position + linesList.Count * 8;
                List<long> offlist = new List<long>();
                for (int i = 0; i < stringOffsetCount; i++)
                {
                    binwr.Write(strOffsets);
                    offlist.Add(strOffsets);
                    strOffsets += Encoding.Unicode.GetByteCount(linesList[i].str);           
                }

                foreach (FmgString line in linesList)
                {
                    binwr.Write(Encoding.Unicode.GetBytes(line.str));
                }

                fileSize = (uint)binwr.BaseStream.Length;

                binwr.BaseStream.Position = 0;
                binwr.Write(idk);
                binwr.Write(fileSize);
                binwr.Write(idk2);
                binwr.Write(idRangeCount);
                binwr.Write(stringOffsetCount);
                binwr.Write(idk3);
                binwr.Write(stringOffsetsOffset);
                binwr.Write(idk4);
                binwr.Write(idk5);
                                
                fmgByteData = data.ToArray();
            }
            Console.Write(".");
            //string fil = Path.GetFileName(fileName.Substring(0,fileName.Length-1));
            //File.WriteAllBytes(@"C:\Users\Burk\Desktop\darksoulsShit\BinderTool v0.4.3\data1\msg\engUS\" + fil, fmgByteData);
        }

        public void GetFromRawData(string fmgPath)
        {
            fmgByteData = File.ReadAllBytes(fmgPath);
            fileName = @"N:\FDP\data\INTERROOT_win64\msg\engUS\64bit\"+Path.GetFileName(fmgPath);
            fileSize = (uint)fmgByteData.Length;
        }
        
    }
}
