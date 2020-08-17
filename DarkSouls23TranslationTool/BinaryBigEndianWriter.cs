using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class BinaryBigEndianWriter : BinaryWriter
    {
        public BinaryBigEndianWriter(Stream output) : base(output)
        {
        }

        public override void Write(uint value)
        {
            byte[] valueBytes = BitConverter.GetBytes(value);
            ToBigEndian(valueBytes);
            base.Write(valueBytes);
        }

        void ToBigEndian(byte[] val)
        {
            Array.Reverse(val);
        }
    }
}
