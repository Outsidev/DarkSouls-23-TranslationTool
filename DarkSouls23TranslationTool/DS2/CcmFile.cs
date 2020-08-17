using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DarkSouls23TranslationTool.DS2
{
    class CcmFile : BndEntry
    {

        struct HeadIdk
        {
            int head;
            int filesize;
            int idk1;
            int idk2;
            int altThingCount;
            int same1;
            int altThingOffset;
            int idk3;
        };
        
        struct PosThing
        {
            public Int16 topX;
            public Int16 topY;
            public Int16 botX;
            public Int16 botY;
        }
        
        struct CharThing
        {
            public char character;
            public int charCode;
            public int posOffset;
            public Int16 textureNo;
            public Int16 spaceBetween;
            public Int16 width;
            public Int16 heigth;
            public long zeros;
        }

        Dictionary<int, PosThing> posDict;
        List<CharThing> charList;

        int fileSize;
        Int16 posCount;
        int charCount;
        int posOffset;
        int charOffset;

        public CcmFile()
        {
        }

        public CcmFile(string _path)
        {
            filePath = _path;
            fileName = Path.GetFileName(filePath);
            data = File.ReadAllBytes(filePath);
        }

        public void ReadandDumpCcm(string orgCcmPath)
        {
            MemoryStream ccmData = new MemoryStream();
            using (BinaryReader binred = new BinaryReader(File.Open(orgCcmPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Unicode))
            {
                binred.BaseStream.Position += 4;
                fileSize = binred.ReadInt32();
                binred.BaseStream.Position += 6;
                posCount = binred.ReadInt16();
                charCount = binred.ReadInt32();
                posOffset = binred.ReadInt32();
                charOffset = binred.ReadInt32();

                binred.BaseStream.Position = 32;
                posDict = new Dictionary<int, PosThing>();
                for (int i = 0; i < posCount; i++)
                {
                    PosThing pp = new PosThing();
                    pp.topX = binred.ReadInt16();
                    pp.topY = binred.ReadInt16();
                    pp.botX = binred.ReadInt16();
                    pp.botY = binred.ReadInt16();
                    int pos = 32 + i * 8;
                    posDict.Add(pos, pp);
                }

                charList = new List<CharThing>();
                for (int i = 0; i < charCount; i++)
                {
                    CharThing cc = new CharThing();
                    cc.character = binred.ReadChar();binred.ReadInt16();                    
                    cc.posOffset = binred.ReadInt32();
                    cc.textureNo = binred.ReadInt16();
                    cc.spaceBetween = binred.ReadInt16();
                    cc.width = binred.ReadInt16();
                    cc.heigth = binred.ReadInt16();
                    cc.zeros = binred.ReadInt64();
                    charList.Add(cc);
                }

                XDocument doc = new XDocument();
                XElement root = new XElement("chrs");
                doc.AddFirst(root);
                for (int i = 0; i < charList.Count; i++)
                {
                    CharThing cc = charList[i];
                    PosThing pp = posDict[cc.posOffset];
                    XElement xelem = new XElement("chr",
                        new XElement("character", cc.character),
                        new XElement("topX", pp.topX),
                        new XElement("topY", pp.topY),
                        new XElement("bottomX", pp.botX),
                        new XElement("bottomY", pp.botY),
                        new XElement("width", cc.width),
                        new XElement("heigth", cc.heigth),
                        new XElement("spaceBetween", cc.spaceBetween),
                        new XElement("textureNo", cc.textureNo),
                        new XElement("posOffset", cc.posOffset)
                        );
                    root.Add(xelem);
                }

                doc.Save(orgCcmPath + ".xml");

            }
        }

        public void CreateCcm(string orgCcmPath, string xmlPath )
        {
            XDocument xml = XDocument.Load(xmlPath);
            charList = new List<CharThing>();
            posDict = new Dictionary<int, PosThing>();
            bool getSpace = false;
            foreach (var chr in xml.Descendants("chr"))
            {
                CharThing cc    = new CharThing();

                if (!getSpace)
                {
                    cc.character = ' ';
                    getSpace = true;
                }else
                    cc.character    = chr.Element("character").Value.ToString()[0];
                cc.width        = short.Parse(chr.Element("width").Value.ToString());
                cc.heigth       = short.Parse(chr.Element("heigth").Value.ToString());
                cc.spaceBetween = short.Parse(chr.Element("spaceBetween").Value.ToString());
                cc.textureNo    = short.Parse(chr.Element("textureNo").Value.ToString());
                cc.posOffset    = int.Parse(chr.Element("posOffset").Value.ToString());

                PosThing pp = new PosThing();
                pp.topX = short.Parse(chr.Element("topX").Value.ToString());
                pp.topY = short.Parse(chr.Element("topY").Value.ToString());
                pp.botX = short.Parse(chr.Element("bottomX").Value.ToString());
                pp.botY = short.Parse(chr.Element("bottomY").Value.ToString());

                charList.Add(cc);
                if(!posDict.ContainsKey(cc.posOffset))
                    posDict.Add(cc.posOffset, pp);
            }


            charList = charList.OrderBy(p => p.character).ToList();
            
            MemoryStream newccmdata = new MemoryStream();
            newccmdata.Write(File.ReadAllBytes(orgCcmPath), 0, 32+posDict.Count*8);
            using (BinaryWriter binwr = new BinaryWriter(newccmdata, Encoding.Unicode))
            {                
                for (int i = 0; i < charList.Count; i++)
                {
                    CharThing cc = charList[i];
                    PosThing pp = posDict[cc.posOffset];
                    if(cc.posOffset == 0)
                    {
                        cc.posOffset = posDict.Max(p => p.Key) + 8;
                        charList[i] = cc;
                    }
                    binwr.BaseStream.Position = cc.posOffset;
                    binwr.Write(pp.topX);
                    binwr.Write(pp.topY);
                    binwr.Write(pp.botX);
                    binwr.Write(pp.botY);
                }
                binwr.BaseStream.Position = 32 + posDict.Count * 8;
                for (int i = 0; i < charList.Count; i++)
                {
                    CharThing cc = charList[i];
                    binwr.Write(cc.character);
                    binwr.Write((short)0);
                    binwr.Write(cc.posOffset);
                    binwr.Write(cc.textureNo);
                    binwr.Write(cc.spaceBetween);
                    binwr.Write(cc.width);
                    binwr.Write(cc.heigth);
                    binwr.Write((long)0);
                }
                binwr.BaseStream.Position = 4;
                binwr.Write((int)binwr.BaseStream.Length);
                binwr.BaseStream.Position = 14;
                binwr.Write((short)posDict.Count);
                binwr.Write(charList.Count);
                binwr.Write(32);
                binwr.Write(32+posDict.Count*8);
            }

            filePath = orgCcmPath;
            fileName = Path.GetFileName(filePath);
            data = newccmdata.ToArray();
            //File.WriteAllBytes(orgCcmPath.Substring(0, orgCcmPath.LastIndexOf(".")), newccmdata.ToArray());

        }
    }
}
