using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{    
    //Big Endian
    class DcxFile
    {
        uint dcx = 0x44435800;//DCX0
        uint idk = 65536;
        uint dcsOffset   = 24;
        uint dcpOffset   = 36;
        uint dcaOffset   = 68;
        uint dataOffset  = 76;

        uint dcs         = 0x44435300;
        uint uncompressedSize;
        uint compressedSize;

        uint dcp         = 0x44435000;
        uint dflt        = 0x44464C54;
        uint headerSize  = 32;
        uint level       = 150994944; //??
        //skip 16byte

        uint dca        = 0x44434100;
        uint idk2       = 8;

        public DcxFile()
        {           
        }

        public void CreateDcx(byte[] dataBytes, string createLocation)
        {
            byte[] decompressedData = dataBytes;
            Deflater df = new Deflater(Deflater.BEST_COMPRESSION);
            MemoryStream compressedDataMem = new MemoryStream();
            using (DeflaterOutputStream compressor = new DeflaterOutputStream(compressedDataMem, df))
            {
                compressor.Write(decompressedData, 0, decompressedData.Length);
            }

            byte[] compressedData = compressedDataMem.ToArray();
            compressedSize = (uint)compressedData.Length;
            uncompressedSize = (uint)decompressedData.Length;

            MemoryStream dcxData = new MemoryStream();
            using (BinaryBigEndianWriter binwr = new BinaryBigEndianWriter(dcxData))
            {
                binwr.Write(dcx);
                binwr.Write(idk);
                binwr.Write(dcsOffset);
                binwr.Write(dcpOffset);
                binwr.Write(dcaOffset);
                binwr.Write(dataOffset);

                binwr.Write(dcs);
                binwr.Write(uncompressedSize);
                binwr.Write(compressedSize);

                binwr.Write(dcp);
                binwr.Write(dflt);
                binwr.Write(headerSize);
                binwr.Write(level);

                long hold = binwr.BaseStream.Position;
                while (binwr.BaseStream.Position < hold + 12)
                    binwr.Write((byte)0);
                binwr.Write((uint)65792);

                binwr.Write(dca);
                binwr.Write(idk2);
                binwr.Write(compressedData.ToArray());
            }
            Console.Write(".");
            File.WriteAllBytes(createLocation, dcxData.ToArray());
        }

    }
}
