using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class ExePatcher
    {
        string exePath;

        public ExePatcher(string _filePath)
        {
            exePath = _filePath;
        }

        public void Patch(string[] _searchArray)
        {
            MakeBackup();
            List<byte[]> searchBytesList = new List<byte[]>();
            for (int i = 0; i < _searchArray.Length; i++)
            {
                searchBytesList.Add(Encoding.Unicode.GetBytes(_searchArray[i]));
            }

            byte[] exeData = File.ReadAllBytes(exePath+".BAK");
            MemoryStream patchedData = new MemoryStream(exeData);
            using (BinaryReader binred = new BinaryReader(patchedData,Encoding.Unicode))
            {
                using(BinaryWriter binwr = new BinaryWriter(patchedData,Encoding.Unicode))
                {
                    for (int i = 0; i < searchBytesList.Count; i++)
                    {
                        SearchAndReplace(binred, binwr, searchBytesList[i]);
                        Console.WriteLine(_searchArray[i] + "..Patched");
                    }
                }
            }

            File.WriteAllBytes(exePath, patchedData.ToArray());
        }

        void SearchAndReplace(BinaryReader binred, BinaryWriter binwr, byte[] searchBytes)
        {
            for (int i = 0; i < binred.BaseStream.Length - searchBytes.Length; i++)
            {
                binred.BaseStream.Position = i;
                byte[] curBytes = binred.ReadBytes(searchBytes.Length);
                if (Tools.CompareBytes(curBytes, searchBytes))
                {
                    int replaceIndex = Array.IndexOf(curBytes, (byte)0x3A);
                    binred.BaseStream.Position = i+replaceIndex;
                    binwr.Write((byte)0x78);
                    break;
                }                
            }
        }

        bool MakeBackup()
        {
            if (!File.Exists(exePath + ".BAK"))
            {
                File.Move(exePath, exePath + ".BAK");
                return true;
            }

            return false;
        }
    }
}
