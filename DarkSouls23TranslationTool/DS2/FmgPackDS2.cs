using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSouls23TranslationTool.DS2
{
    class FmgPackDS2
    {
        public List<FmgFileDS2> fmgList;

        public FmgPackDS2()
        {
            fmgList = new List<FmgFileDS2>();
        }

        public void ReadLinesFromFmgFolder(string fmgFolderPath)
        {
            string[] fmgPaths = Directory.GetFiles(fmgFolderPath, "*.fmg", SearchOption.AllDirectories);
            List<string> fmgLines = new List<string>();

            foreach(string path in fmgPaths)
            {
                FmgFileDS2 fmg = new FmgFileDS2();
                fmg.orginalPath = path.Remove(path.IndexOf(fmgFolderPath), fmgFolderPath.Length+1);
                fmg.ReadFmg(path);
                fmgList.Add(fmg);
            }            

        }

        public void EditFmgsFromExcel(string fmgFolder, List<FmgFileDS2> fmgListfromExcel)
        {
            fmgList = fmgListfromExcel;
            foreach (FmgFileDS2 fmg in fmgList)
            {
                fmg.WriteFmg(Path.Combine(fmgFolder, fmg.orginalPath));
            }

            foreach (FmgFileDS2 fmg in fmgList)
            {
                File.WriteAllBytes(Path.Combine(fmgFolder, fmg.orginalPath), fmg.fmgByteData);
            }

        }


    }
}
