using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Data;
using System.Data.SqlClient;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.Common;

using OpenXmlPowerTools;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Text;

namespace PhotoBookmart.Controllers
{
    public class StagingController : Controller
    {
        public ActionResult Index()
        {
            return Content("Here is staging controller!");
        }
        
        public void ExportFileWordToHttpResponse()
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachmnet;filename=GridToWord.doc");
            Response.Charset = "";
            Response.ContentType = "application/vn.ms-word";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            hw.Write("<b>Email</b>:<a rel='email' href='mailto:npe.etc@gmail.com'>npe.etc@gmail.com</a>");

            Response.Output.Write(sw);
            Response.Flush();
            Response.Close();
            Response.End();
        }
        
        public void ExportFileExcelToHttpResponse()
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachmnet;filename=GridToExcel.xls");
            Response.Charset = "";
            Response.ContentType = "application/vn.ms-excel";

            StringWriter sw = new StringWriter();
            HtmlTextWriter hw = new HtmlTextWriter(sw);
            hw.Write("<b>Email</b>:<a rel='email' href='mailto:npe.etc@gmail.com'>npe.etc@gmail.com</a>");

            Response.Output.Write(sw);
            Response.Flush();
            Response.Close();
            Response.End();
        }

        public void ExportFileExcelWithOpenXML()
        {
            MemoryStream ms = new MemoryStream();
            SpreadsheetDocument xl = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            WorkbookPart wbp = xl.AddWorkbookPart();
            WorksheetPart wsp = wbp.AddNewPart<WorksheetPart>();
            Workbook wb = new Workbook();
            FileVersion fv = new FileVersion();
            fv.ApplicationName = "Microsoft Office Excel";
            Worksheet ws = new Worksheet();

            //First cell
            SheetData sd = new SheetData();
            Row r1 = new Row() { RowIndex = (UInt32Value)1u };
            Cell c1 = new Cell();
            c1.DataType = CellValues.String;
            c1.CellValue = new CellValue("some value");
            r1.Append(c1);

            // Second cell
            Cell c2 = new Cell();
            c2.CellReference = "C1";
            c2.DataType = CellValues.String;
            c2.CellValue = new CellValue("other value");
            r1.Append(c2);
            sd.Append(r1);

            //third cell
            Row r2 = new Row() { RowIndex = (UInt32Value)2u };
            Cell c3 = new Cell();
            c3.DataType = CellValues.String;
            c3.CellValue = new CellValue("some string");
            r2.Append(c3);
            sd.Append(r2);

            ws.Append(sd);
            wsp.Worksheet = ws;
            wsp.Worksheet.Save();
            Sheets sheets = new Sheets();
            Sheet sheet = new Sheet();
            sheet.Name = "first sheet";
            sheet.SheetId = 1;
            sheet.Id = wbp.GetIdOfPart(wsp);
            sheets.Append(sheet);
            wb.Append(fv);
            wb.Append(sheets);

            xl.WorkbookPart.Workbook = wb;
            xl.WorkbookPart.Workbook.Save();
            xl.Close();
            string fileName = "testOpenXml.xlsx";
            Response.Clear();
            byte[] dt = ms.ToArray();

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            Response.BinaryWrite(dt);
            Response.End();
        }

        public void ExportFileWithOpenXML1()
        {
            string path = Server.MapPath("~/Reports/BienDong.docx");
            // if (Util.IsWordprocessingML(ext)) { }
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(System.IO.File.ReadAllBytes(path), 0, System.IO.File.ReadAllBytes(path).Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(ms, true))
                {
                    var body = wDoc.MainDocumentPart.Document.Body;
                    var lst_para = body.Elements<Paragraph>().LastOrDefault();
                    var new_para = new Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text("Hello world!")));
                    XElement p = new XElement("p", "&nbsp;&nbsp;Hello");
                    lst_para.InsertAfterSelf(new_para);
                    lst_para.Append( = string.Format("{0}", p);
                }
                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachmnet; filename=ExportFileWithOpenXML1.docx");
                Response.ContentType = "application/octet-stream";
                Response.BinaryWrite(ms.ToArray());
                Response.Flush();
                Response.End();
            }
        }

        public void ExportFileWithOpenXML2()
        {
            string path = Server.MapPath("~/Reports/BienDong.docx");
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(System.IO.File.ReadAllBytes(path), 0, System.IO.File.ReadAllBytes(path).Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(ms, true))
                {
                    var body = wDoc.MainDocumentPart.Document.Body;
                    var lastPara = body.Elements<Paragraph>().LastOrDefault();
                    var newPara = new Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text("Hello world!")));
                    lastPara.InsertAfterSelf(newPara);
                }
            }
        }

        public void ExportFileWithOpenXML3()
        {
            string path_file_docx = Server.MapPath("~/Reports/BienDong.docx");
            string path_dir_export_relative = "Report/Export";
            string path_dir_export_absolute = Server.MapPath(string.Format("~/{0}", path_dir_export_relative));

            using (MemoryStream ms = new MemoryStream())
            {
                DirectoryInfo dir_info = new DirectoryInfo(path_dir_export_absolute);
                if (!dir_info.Exists)
                {
                    dir_info.Create();
                }

                string name_file_html = string.Format("{0}.html", Guid.NewGuid());
                ConvertToHtml(System.IO.File.ReadAllBytes(path_file_docx), dir_info, name_file_html);
                Response.Redirect(string.Format("/{0}/{1}", path_dir_export_relative, name_file_html));
            }
        }

        public static void ConvertToHtml(byte[] byteArray, DirectoryInfo desDirectory, string htmlFileName)
        {
            FileInfo fiHtml = new FileInfo(Path.Combine(desDirectory.FullName, htmlFileName));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(byteArray, 0, byteArray.Length);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var imageDirectoryFullName = fiHtml.FullName.Substring(0, fiHtml.FullName.Length - fiHtml.Extension.Length) + "_files";
                    var imageDirectoryRelativeName = fiHtml.Name.Substring(0, fiHtml.Name.Length - fiHtml.Extension.Length) + "_files";
                    int imageCounter = 0;
                    var pageTitle = (string)wDoc
                        .CoreFilePropertiesPart
                        .GetXDocument()
                        .Descendants(DC.title)
                        .FirstOrDefault();
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        PageTitle = pageTitle,
                        FabricateCssClasses = true,
                        CssClassPrefix = "pt-",
                        RestrictToSupportedLanguages = false,
                        RestrictToSupportedNumberingFormats = false,
                        ImageHandler = imageInfo =>
                        {
                            DirectoryInfo localDirInfo = new DirectoryInfo(imageDirectoryFullName);
                            if (!localDirInfo.Exists) { localDirInfo.Create(); };
                            ++imageCounter;
                            string extensions = imageInfo.ContentType.Split('/')[1].ToLower();
                            ImageFormat imageFormat = null;
                            if (extensions == "png")
                            {
                                extensions = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extensions == "gif")
                            {
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extensions == "bmp")
                            {
                                imageFormat = ImageFormat.Bmp;
                            }
                            else if (extensions == "jpeg")
                            {
                                imageFormat = ImageFormat.Jpeg;
                            }
                            else if (extensions == "tiff")
                            {
                                extensions = "gif";
                                imageFormat = ImageFormat.Gif;
                            }
                            else if (extensions == "wmf")
                            {
                                extensions = "wmf";
                                imageFormat = ImageFormat.Wmf;
                            }

                            if (imageFormat == null) { return null; }

                            FileInfo imageFileName = new FileInfo(imageDirectoryFullName + "/image" + imageCounter.ToString() + "." + extensions);
                            try
                            {
                                imageInfo.Bitmap.Save(imageFileName.FullName, imageFormat);
                            }
                            catch (System.Runtime.InteropServices.ExternalException)
                            {
                                return null;
                            }
                            XElement img = new XElement(Xhtml.img,
                                new XAttribute(NoNamespace.src, imageDirectoryRelativeName + "/" + imageFileName.Name),
                                imageInfo.ImgStyleAttribute,
                                imageInfo.AltText != null ?
                                    new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
                            return img;
                        }
                    };
                    XElement html = HtmlConverter.ConvertToHtml(wDoc, settings);
                    var body = html.Descendants(Xhtml.body).First();
                    body.AddFirst(
                        new XElement(Xhtml.p,
                            new XElement(Xhtml.A,
                                new XAttribute("href", "/WebForm1.aspx"), "Go back to Upload Page")));

                    var htmlString = html.ToString(SaveOptions.DisableFormatting);
                    System.IO.File.WriteAllText(fiHtml.FullName, htmlString, Encoding.UTF8);
                }
            }
        }
    }
}
