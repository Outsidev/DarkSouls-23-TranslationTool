using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class TextFile
    {
        public string filePath;
        public List<TextBlock> blocks;

        public TextFile(string _filePath)
        {
            filePath = _filePath;
            blocks = new List<TextBlock>();
        }

        public void Create(FmgFile[] fmgArray)
        {
            //yazdır
            StringBuilder allLines = new StringBuilder();
            foreach (FmgFile fmg in fmgArray)
            {
                allLines.AppendLine(fmg.fileId + "," + fmg.fileName);
                for (int i = 0; i < fmg.linesList.Count; i++)
                {
                    FmgString str = fmg.linesList[i];
                    string removedSpecials = str.str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
                    allLines.AppendLine(str.id + "," + str.firstorLast + "," + removedSpecials);
                }
                allLines.AppendLine();
                allLines.AppendLine();
            }

            File.WriteAllText(filePath + ".txt", allLines.ToString());
        }

        public void Parse()
        {
            string[] allLines = File.ReadAllLines(filePath);
            
            TextBlock curBlock;
            int i = 0;
            while(i < allLines.Length)
            {                        
                if(allLines[i].Contains(@"N:\FDP\"))
                {                                        
                    curBlock = new TextBlock();
                    int idComaPos = allLines[i].IndexOf(",");
                    curBlock.fileId = int.Parse(allLines[i].Substring(0, idComaPos));
                    curBlock.pathName = allLines[i].Substring(idComaPos+1)+"\0";
                    i++;
                    while (i < allLines.Length && allLines[i] != "")
                    {
                        string lin = allLines[i];
                        int idPos = lin.IndexOf(',');
                        int foLPos = lin.IndexOf(',', idPos+1);
                        int id = int.Parse(lin.Substring(0, idPos));
                        int firstOrLast = int.Parse(lin.Substring(idPos+1,foLPos-idPos-1));
                        string str = lin.Substring(foLPos + 1);
                        FmgString fmgstr = new FmgString();
                        fmgstr.id = (uint)id;
                        fmgstr.str = str.Replace("\\n","\n").Replace("\\r","\r").Replace("\\t","\t") + "\0";
                        fmgstr.firstorLast = firstOrLast;
                        curBlock.lines.Add(fmgstr);
                        i++;
                    }
                    blocks.Add(curBlock);
                    FmgIdRange idrange = new FmgIdRange();
                    int k = 0;
                    while (k < curBlock.lines.Count)
                    {
                        if (curBlock.lines[k].firstorLast == 1)
                        {
                            idrange = new FmgIdRange();
                            idrange.firstId = curBlock.lines[k].id;
                            idrange.offsetIndex = (uint)k;
                            while (k+1 < curBlock.lines.Count && curBlock.lines[k+1].firstorLast < 1)
                                k++;                         
                            idrange.lastId = curBlock.lines[k].id;
                            curBlock.idRanges.Add(idrange);
                        }
                        k++;
                    }
                }
                i++;                                
            }            
        }
    }
}
