using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool.DS2
{
    class FmgFileDS2
    {
        public List<string> linesList;
        public string orginalPath;
        public byte[] fmgByteData;

        public FmgFileDS2()
        {
            linesList = new List<string>();
        }

        public void ReadFmg(string fmgPath)
        {
            using (BinaryReader binred = new BinaryReader(File.Open(fmgPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Unicode))
            {
                binred.BaseStream.Position = 16;
                int strOffsetCount = binred.ReadInt32();
                int strOffsetsStartPos = binred.ReadInt32();
                int strStartPo = strOffsetsStartPos + strOffsetCount * 4;
                binred.BaseStream.Position = strOffsetsStartPos;
                for(int i=0; i < strOffsetCount; i++)
                {                    
                    int strOffset = binred.ReadInt32();
                    if (strOffset == 0)
                        continue;

                    long hold = binred.BaseStream.Position;

                    binred.BaseStream.Position = strOffset;
                    string line = Tools.ReadCharTillNull(binred);
                    linesList.Add(line);

                    binred.BaseStream.Position = hold;
                }
            }
        }

        public void WriteFmg(string orgFmgPath)
        {
            
            MemoryStream memData = new MemoryStream();
            using (BinaryReader binred = new BinaryReader(File.Open(orgFmgPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {                
                using (BinaryWriter binwr = new BinaryWriter(memData))
                {
                    binred.BaseStream.Position = 16;
                    int strOffsetCount = binred.ReadInt32();
                    int strOffsetsStartPos = binred.ReadInt32();
                    int strStartOffset = strOffsetsStartPos + strOffsetCount * 4;

                    binred.BaseStream.Position = 0;
                    binwr.Write(binred.ReadBytes(strOffsetsStartPos));

                    int strOffsets = strStartOffset;
                    int i = 0;
                    while(i < linesList.Count)
                    {
                        if (binred.ReadInt32() == 0)
                        {
                            binwr.Write((int)0);
                            continue;
                        }

                        int linelen = Encoding.Unicode.GetByteCount(linesList[i]);
                        binwr.Write(strOffsets);
                        strOffsets += linelen;
                        i++;
                    }

                    while (binwr.BaseStream.Position < strStartOffset)
                        binwr.Write(0);

                    for (i = 0; i < linesList.Count; i++)
                    {
                        byte[] what = Encoding.Unicode.GetBytes(linesList[i]);
                        binwr.Write(what);
                    }

                    binwr.BaseStream.Position = 4;
                    binwr.Write((int)memData.Length);
                    binwr.Write((int)1);
                }
            }

            fmgByteData = memData.ToArray();
        }

    }
}
