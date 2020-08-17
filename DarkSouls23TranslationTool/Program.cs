using DarkSouls23TranslationTool.DS2;
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
    class Program
    {
        public static string filePath;
        public static string fileExtension;
        static void Main(string[] args)
        {
            filePath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\enc_regulation.bnd.dcx";//args[0];
            fileExtension = Path.GetExtension(filePath);

            //DS3Functions();
            DS2Functions();
            
        }       
        
        public static void DS3Functions()
        {
            if (fileExtension == ".msgbnd")
            {
                Console.WriteLine("To Excel: " + filePath);
                BndFile bnd = new BndFile();
                bnd.ExtractTexts(filePath);
                Console.WriteLine("OK!");
            }
            else if (fileExtension == ".xlsx")
            {
                Console.WriteLine("To Dcx: " + filePath);
                BndFile bndFile = new BndFile();
                bndFile.ImportTexts(filePath);
                DcxFile dcxFile = new DcxFile();
                dcxFile.CreateDcx(bndFile.bndByteData, filePath.Substring(0, filePath.LastIndexOf(".")) + ".dcx");
                Console.WriteLine("OK!");
            }
            else if (fileExtension == ".exe")
            {
                Console.WriteLine("Patch exe: " + filePath);
                ExePatcher exepatch = new ExePatcher(filePath);
                string[] targets = new string[] { "font:/%s/", "msg:/%s/item_dlc2", "msg:/%s/menu_dlc2", "menu:/%s\0\0" };
                exepatch.Patch(targets);
            }
            else if (fileExtension == String.Empty)
            {
                string ddsFolder = filePath;
                string tpfFilePath = ddsFolder + ".tpf";
                if (!File.Exists(tpfFilePath))
                {
                    Console.WriteLine(Path.GetFileName(tpfFilePath) + " dosyası yok!");
                    return;
                }

                TpfFile tpf = new TpfFile();
                tpf.Create(ddsFolder, tpfFilePath);
                DcxFile dcxFile = new DcxFile();
                dcxFile.CreateDcx(tpf.data, tpfFilePath + ".dcx");
            }

        }

        public static void DS2Functions()
        {
            if (fileExtension == ".exe")
            {
                Console.WriteLine("Patch exe NOT WORKS FOR NOW... " + filePath);
                ExePatcher exepatch = new ExePatcher(filePath);
                //string[] targets = new string[] { "font:/%s/", "msg:/%s/item_dlc2", "msg:/%s/menu_dlc2", "menu:/%s\0\0" };
                //exepatch.Patch(targets);
            }
            else if (fileExtension == string.Empty)
            {
                if (File.Exists(filePath + ".fetexbnd") && Directory.GetFiles(filePath, "*.tpf", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    //Dark Souls 2 Texture DDS container : fetexbnd (You win, died etc files)
                    Console.WriteLine("Tex, DDS to Fetexbnd.dcx: " + filePath);
                    string tpfFolder = filePath;
                    string fetexFilePath = tpfFolder + ".fetexbnd";

                    Fetexbnd fetex = new Fetexbnd();
                    fetex.CreateFetexFromDDSs(tpfFolder);
                    DcxFile dcxman = new DcxFile();
                    dcxman.CreateDcx(fetex.bndByteData, fetexFilePath + ".dcx");
                }
                else if(File.Exists(filePath+".fontbnd") && Directory.GetFiles(filePath, "*.ccm", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    //Dark Souls 2 FontTextures DDS container : fontbnd (all font dds files)
                    Console.WriteLine("Font, DDS+CCM to Fontbnd.dcx: " + filePath);
                    string exportedFolder = filePath;
                    string fontFilePath = exportedFolder + ".fontbnd";

                    Fontbnd font = new Fontbnd();
                    font.CreateFontFromFiles(exportedFolder);
                    DcxFile dcxman = new DcxFile();
                    dcxman.CreateDcx(font.bndByteData, fontFilePath + ".dcx");
                }
                else if(Directory.GetFiles(filePath,"*.fmg", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    //DS2 Fmg, subtitles export to excel
                    Console.WriteLine("Subs, Fmgs to Excel: " + filePath);
                    string exportedFolder = filePath;
                    string fontFilePath = exportedFolder + ".fontbnd";

                    FmgPackDS2 fmgPack = new FmgPackDS2();
                    fmgPack.ReadLinesFromFmgFolder(filePath);
                    ExcelFile excel = new ExcelFile();
                    excel.CreateDS2(fmgPack.fmgList, filePath + ".xlsx");
                }
            }
            else if(fileExtension == ".ccm")
            {
                Console.WriteLine("Font, Ccm to Xml: " + filePath);
                string ccmPath = filePath;
                CcmFile ccm = new CcmFile();
                ccm.ReadandDumpCcm(ccmPath);
            }
            else if(fileExtension == ".xlsx")
            {
                Console.WriteLine("Subs, Excel to Fmgs: " + filePath);
                string excelPath = filePath;
                string fmgFolder = Path.GetFileNameWithoutExtension(excelPath);
                
                ExcelFile excel = new ExcelFile();
                FmgPackDS2 fmgPack = new FmgPackDS2();
                List<FmgFileDS2> fmgFiles = excel.ReadDS2(excelPath);
                fmgPack.EditFmgsFromExcel(fmgFolder, fmgFiles);
            }

            
        }
    }
}
