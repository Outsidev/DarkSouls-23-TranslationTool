using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class Tools
    {

        public static string ReadCharTillNull(BinaryReader binred)
        {
            List<char> bb8 = new List<char>();
            char bb;
            while (binred.BaseStream.Position < binred.BaseStream.Length && (bb = binred.ReadChar()) != 0x0)
            {
                bb8.Add(bb);
            }
            return new string(bb8.ToArray());
        }

        public static byte[] ReadByteTillNull(BinaryReader binred)
        {
            List<byte> bb8 = new List<byte>();
            byte bb;
            while (binred.BaseStream.Position < binred.BaseStream.Length && (bb = binred.ReadByte()) != 0x0)
            {
                bb8.Add(bb);
            }
            return bb8.ToArray();
        }

        public static string HandleCellValue(Object val)
        {
            if (val == null)
                return "";

            return val.ToString();
        }

        public static bool CompareBytes(byte[] b1, byte[] b2)
        {
            for (int i = 0; i < b1.Length; i++)
            {
                if(b1[i] != b2[i])
                    return false;
            }

            return true;
        }

        public static string GetFolderName(string folderPath)
        {
            return folderPath.Substring(folderPath.LastIndexOf("\\")+1);
        }

        public static string GetFilePathWithoutExtension(string filePath)
        {
            return filePath.Substring(0, filePath.LastIndexOf("."));
        }

    }
}
