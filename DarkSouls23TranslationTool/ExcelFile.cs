using DarkSouls23TranslationTool.DS2;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool
{
    class ExcelFile
    {
        public string excelPath;
        public List<FmgFile> fmgBlocks;

        public ExcelFile()
        {
        }

        public void Create(FmgFile[] fmgArray, string where)
        {
            ExcelPackage pck = new ExcelPackage(File.Open(where, FileMode.Create));
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Satirlar");
            ws.Cells["A1"].Value = "Orjinal Satır";
            ws.Cells["B1"].Value = "Türkçe Satır";
            ws.Cells["C1"].Value = "ID";
            ws.Cells["D1"].Value = "FOL";
            ws.Cells["E1"].Value = "File";
            ws.Cells["F1"].Value = "FileID";
            ws.Cells["A1:F1"].Style.Font.Bold = true;

            int col = 2;
            foreach(FmgFile fmg in fmgArray)
            {
                for (int i = 0; i < fmg.linesList.Count; i++)
                {
                    FmgString str = fmg.linesList[i];
                    string removedSpecials = str.str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
                    ws.Cells["A" + col].Value = removedSpecials;
                    ws.Cells["C" + col].Value = str.id;
                    ws.Cells["D" + col].Value = str.firstorLast;
                    ws.Cells["E" + col].Value = fmg.fileName;
                    ws.Cells["F" + col].Value = fmg.fileId;
                    col++;                    
                }
            }

            Console.Write(".");
            pck.Save();
            pck.Dispose();
        }

        public void CreateDS2(List<FmgFileDS2> fmgList, string where)
        {
            ExcelPackage pck = new ExcelPackage(File.Open(where, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite));
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Satirlar");
            ws.Cells["A1"].Value = "Orjinal Satır";
            ws.Cells["B1"].Value = "Türkçe Satır";
            ws.Cells["C1"].Value = "Path";
            ws.Cells["A1:C1"].Style.Font.Bold = true;

            int col = 2;
            foreach (FmgFileDS2 fmg in fmgList)
            {
                for (int i = 0; i < fmg.linesList.Count; i++)
                {
                    string str = fmg.linesList[i];
                    string removedSpecials = str.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
                    ws.Cells["A" + col].Value = removedSpecials;
                    ws.Cells["C" + col].Value = fmg.orginalPath;
                    col++;
                }
            }

            Console.Write(".");
            pck.Save();
            pck.Dispose();
        }


        public void Read(string where)
        {
            excelPath = where;
            ExcelPackage pck = new ExcelPackage(File.Open(where, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            ExcelWorksheet ws = pck.Workbook.Worksheets["Satirlar"];

            Dictionary<string, FmgFile> fmgDic = new Dictionary<string, FmgFile>();
            for (int col = 2; col < ws.Dimension.Rows; col++)
            {
                string orgStr   = Tools.HandleCellValue(ws.Cells["A" + col].Value);
                string trStr    = Tools.HandleCellValue(ws.Cells["B" + col].Value);
                uint id         = uint.Parse(ws.Cells["C" + col].Value.ToString());
                int fol         = int.Parse(ws.Cells["D" + col].Value.ToString());
                string fileName = ws.Cells["E" + col].Value.ToString();
                uint fileId     = uint.Parse(ws.Cells["F" + col].Value.ToString());


                if (!fmgDic.ContainsKey(fileName))
                {
                    FmgFile fmgFile = new FmgFile();
                    fmgFile.fileName    = fileName + "\0";
                    fmgFile.fileId      = fileId;
                    fmgDic.Add(fileName, fmgFile); 
                }

                FmgString fmgStr = new FmgString();
                string selectedStr = trStr;
                if (trStr == null || trStr == "")
                    selectedStr = orgStr;

                selectedStr = selectedStr.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t") + "\0";
                fmgStr.str = selectedStr;
                fmgStr.id = id;
                fmgStr.firstorLast = fol;
                fmgDic[fileName].linesList.Add(fmgStr);

            }            
            fmgBlocks = fmgDic.Values.ToList();

            foreach (var fmg in fmgBlocks)
            {
                FmgIdRange idrange;
                for (int i = 0; i < fmg.linesList.Count; i++)
                {                    
                    FmgString fstr = fmg.linesList[i];
                    if(fstr.firstorLast == 1)
                    {
                        idrange = new FmgIdRange();
                        idrange.firstId = fstr.id;
                        idrange.offsetIndex = (uint)i;
                        idrange.lastId = fstr.id;
                        fmg.idRangesList.Add(idrange);
                    }
                    else if(fstr.firstorLast == -1)
                    {
                        fmg.idRangesList.Last().lastId = fstr.id;
                    }
                }
            }

            Console.Write(".");
        }

        public List<FmgFileDS2> ReadDS2(string where)
        {
            excelPath = where;
            ExcelPackage pck = new ExcelPackage(File.Open(where, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            ExcelWorksheet ws = pck.Workbook.Worksheets["Satirlar"];

            Dictionary<string, FmgFileDS2> fmgDic = new Dictionary<string, FmgFileDS2>();
            for (int col = 2; col < ws.Dimension.Rows; col++)
            {
                string orgStr   = Tools.HandleCellValue(ws.Cells["A" + col].Value);
                string trStr    = Tools.HandleCellValue(ws.Cells["B" + col].Value);
                string path     = ws.Cells["C" + col].Value.ToString();


                if (!fmgDic.ContainsKey(path))
                {
                    FmgFileDS2 fmgFile = new FmgFileDS2();
                    fmgDic.Add(path, fmgFile);
                }

                string selectedStr = trStr;
                if (trStr == null || trStr == "")
                    selectedStr = orgStr;

                selectedStr = selectedStr.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t") + "\0";
                fmgDic[path].orginalPath = path;
                fmgDic[path].linesList.Add(selectedStr);

            }

            return fmgDic.Values.ToList();
        }
    }
}
