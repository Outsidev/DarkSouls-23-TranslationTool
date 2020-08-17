using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class TextBlock
    {
        public string pathName;
        public List<FmgString> lines;
        public List<FmgIdRange> idRanges;
        public int fileId;

        public TextBlock()
        {
            lines = new List<FmgString>();
            idRanges = new List<FmgIdRange>();
        }
    }
}
