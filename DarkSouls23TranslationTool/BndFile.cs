using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{

    class BndFile
    {
        public byte[] bndByteData;
        public string bndFilePath;
        public FmgFile[] fmgArray;        

        uint bnd = 876891714;//bnd4
        uint idka = 0;
        uint idkb = 65536;
        public uint fileCount;
        ulong idk2 = 64;
        ulong version = 59726742435632; //07D7R6..
        uint directoryEntrySize = 36;
        uint idk3 = 0;
        public uint dataOffset;
        uint idk4 = 0;
        long idk5 = 291841;

        public BndFile()
        {
        }

        public void ExtractTexts(string filePath)
        {
            using (BinaryReader binred = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite), Encoding.Unicode))
            {
                bnd = binred.ReadUInt32();
                idka = binred.ReadUInt32();
                idka = binred.ReadUInt32();
                fileCount = binred.ReadUInt32();
                idk2 = binred.ReadUInt64();
                version = binred.ReadUInt64();
                directoryEntrySize = binred.ReadUInt32();
                idk3 = binred.ReadUInt32();
                dataOffset = binred.ReadUInt32();
                idk4 = binred.ReadUInt32();
                binred.ReadInt64();
                binred.ReadInt64();

                fmgArray = new FmgFile[fileCount];
                for (int i = 0; i < fileCount; i++)
                {
                    uint fileEntryOffset;
                    uint fileNameOffset;
                    uint fileId = 1;

                    ulong idk6 = binred.ReadUInt64();                    
                    ulong fileEntrySize = binred.ReadUInt64();
                    if (directoryEntrySize == 36)
                    {
                        binred.ReadUInt64();//fileEntrySize again
                        fileEntryOffset = binred.ReadUInt32();
                        fileId = binred.ReadUInt32();
                        fileNameOffset = binred.ReadUInt32();
                    }
                    else
                    {
                        fileEntryOffset = binred.ReadUInt32();
                        fileNameOffset = binred.ReadUInt32();
                    }

                    long hold = binred.BaseStream.Position;
                    FmgFile entry = new FmgFile();
                    entry.FillData(binred, fileEntryOffset, (int)fileEntrySize, fileNameOffset, fileId);
                    binred.BaseStream.Position = hold;
                    fmgArray[i] = entry;
                }
            }

            ExcelFile excelFile = new ExcelFile();
            string exportPath = filePath + ".xlsx";
            excelFile.Create(fmgArray, exportPath);
        }

        public void ImportTexts(string excelPath)
        {
            bndFilePath = excelPath;
            ExcelFile excelFile = new ExcelFile();
            excelFile.Read(excelPath);

            fmgArray = excelFile.fmgBlocks.ToArray();
            for (int i = 0; i < fmgArray.Length; i++)
                fmgArray[i].WriteByteData();

            fileCount = (uint)fmgArray.Length;
            dataOffset = 0;
            MemoryStream bnddata = new MemoryStream();
            using (BinaryWriter binwr = new BinaryWriter(bnddata, Encoding.Unicode))
            {
                binwr.Write(bnd);
                binwr.Write(idka);
                binwr.Write(idkb);
                binwr.Write(fileCount);
                binwr.Write(idk2);
                binwr.Write(version);
                binwr.Write(directoryEntrySize);
                binwr.Write(idk3);

                long dataOffsetPos = binwr.BaseStream.Position;
                binwr.Write(dataOffset);
                binwr.Write(idk4);

                binwr.Write(idk5);
                //binwr.Write(encoding);
                //binwr.Write((int)1140);
                //binwr.Write(new byte[] { 0, 0, 0 });
                long afterNamesPos = binwr.BaseStream.Position;
                binwr.Write((ulong)0);//idk

                int headerPos = (int)binwr.BaseStream.Position;
                int nameOffsets = (int)binwr.BaseStream.Position + 36 * (int)fileCount;
                for (int i = 0; i < fileCount; i++)
                {                    
                    binwr.Write((int)64);
                    binwr.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
                    binwr.Write((long)fmgArray[i].fileSize);
                    binwr.Write((long)fmgArray[i].fileSize);
                    binwr.Write((int)0);//fileoffset
                    binwr.Write(fmgArray[i].fileId);
                    binwr.Write((int)nameOffsets);//nameoffset
                    nameOffsets += Encoding.Unicode.GetByteCount(fmgArray[i].fileName);
                }

                for (int i = 0; i < fileCount; i++)
                {
                    binwr.Write(Encoding.Unicode.GetBytes(fmgArray[i].fileName));
                }

                long hold = binwr.BaseStream.Position;
                binwr.BaseStream.Position = dataOffsetPos;
                binwr.Write(hold);
                binwr.BaseStream.Position = hold;

                headerPos += 24;//fileOffset
                for (int i = 0; i < fileCount; i++)
                {
                    uint tempOffset = (uint)binwr.BaseStream.Position;
                    binwr.Write(fmgArray[i].fmgByteData);
                    binwr.BaseStream.Position = headerPos+ i * 36;
                    binwr.Write(tempOffset);
                    binwr.BaseStream.Position = tempOffset + fmgArray[i].fmgByteData.Length;                    
                }

            }

            Console.Write(".");
            bndByteData = bnddata.ToArray();
            //File.WriteAllBytes(textPath+".WITHID", bnddata.ToArray());
        }

        public void TestImporter(string bndFilePath)
        {
            string[] fmgPaths = //Directory.GetFiles(Path.GetDirectoryName(filePath), "*.fmg");
                new string[]
                {
                    "会話.fmg",
                    "血文字.fmg",
                    "ムービー字幕.fmg",
                    "イベントテキスト.fmg",
                    "インゲームメニュー.fmg",
                    "メニュー共通テキスト.fmg",
                    "メニューその他.fmg",
                    "ダイアログ.fmg",
                    "キーガイド.fmg",
                    "一行ヘルプ.fmg",
                    "項目ヘルプ.fmg",
                    "テキスト表示用タグ一覧.fmg",
                    "FDP_メニューテキスト.fmg",
                    "FDP_一行ヘルプ.fmg",
                    "FDP_キーガイド.fmg",
                    "FDP_システムメッセージ_win64.fmg",
                    "FDP_ダイアログ.fmg",
                    "FDP_システムメッセージ_ps4.fmg",
                    "FDP_システムメッセージ_xboxone.fmg",
                    "会話_dlc1.fmg",
                    "イベントテキスト_dlc1.fmg",
                    "FDP_メニューテキスト_dlc1.fmg",
                    "FDP_一行ヘルプ_dlc1.fmg",
                    "FDP_システムメッセージ_win64_dlc1.fmg",
                    "FDP_ダイアログ_dlc1.fmg",
                    "FDP_システムメッセージ_ps4_dlc1.fmg",
                    "FDP_システムメッセージ_xboxone_dlc1.fmg",
                    "血文字_dlc1.fmg",
                    "会話_dlc2.fmg",
                    "イベントテキスト_dlc2.fmg",
                    "FDP_メニューテキスト_dlc2.fmg",
                    "FDP_一行ヘルプ_dlc2.fmg",
                    "FDP_システムメッセージ_win64_dlc2.fmg",
                    "FDP_ダイアログ_dlc2.fmg",
                    "FDP_システムメッセージ_ps4_dlc2.fmg",
                    "FDP_システムメッセージ_xboxone_dlc2.fmg",
                    "血文字_dlc2.fmg",
                };
            fmgArray = new FmgFile[fmgPaths.Length];
            string sota = @"C:\Users\Burk\Desktop\darksoulsShit\BinderTool v0.4.3\data1\msg\engUS\";
            for (int i = 0; i < fmgPaths.Length; i++)
            {
                FmgFile fmg = new FmgFile();
                fmg.GetFromRawData(sota + fmgPaths[i]);
                fmg.fileName = fmg.fileName + "\0";
                fmgArray[i] = fmg;
            }

            byte[] bndFile = File.ReadAllBytes(bndFilePath);
            using (BinaryReader binred = new BinaryReader(new MemoryStream(bndFile)))
            {
                MemoryStream newDat = new MemoryStream();
                using(BinaryWriter binwr = new BinaryWriter(newDat))
                {
                    binwr.Write(binred.ReadBytes(6304));
                    for (int i = 0; i < fmgArray.Length; i++)
                    {                        
                        uint hold = (uint)binwr.BaseStream.Position;
                        binwr.BaseStream.Position = 64 + (36 * i + 24);
                        binwr.Write(hold);
                        binwr.BaseStream.Position = hold;
                        binwr.Write(fmgArray[i].fmgByteData);
                    }
                }
                File.WriteAllBytes(bndFilePath+".TESTIMPORTER", newDat.ToArray());
            }            
        }


    }
}
