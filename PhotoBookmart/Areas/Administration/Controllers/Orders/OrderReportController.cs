using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;

using OpenXmlPowerTools;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing; // using DocumentFormat.OpenXml.Spreadsheet;

using PhotoBookmart.Areas.Administration.Controllers;
using PhotoBookmart.Areas.Administration.Models;
using PhotoBookmart.Common.Helpers;
using PhotoBookmart.Controllers;
using PhotoBookmart.DataLayer;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.DataLayer.Models.System;
using PhotoBookmart.DataLayer.Models.Users_Management;

namespace PhotoBookmart.Areas.Administration.Controllers
{
    [ABRequiresAnyRole(RoleEnum.Admin, RoleEnum.Province, RoleEnum.District)]
    public class OrderReportController : WebAdminController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new BaoCao_DSChiTraTroCapModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult BaoCao_DSChiTraTroCap(BaoCao_DSChiTraTroCapModel model)
        {
            if (model.Thang < 1 || model.Thang > 12 || model.Nam < DateTime.MinValue.Year || model.Nam > DateTime.MaxValue.Year || model.LoaiDTs.Count == 0 || model.Villages.Count == 0 ||
                model.LoaiDTs.Count(x => x.Length != 2) != 0 || model.Villages.Count(x => x.Length != GetLenMaHCByRole(RoleEnum.Village)) != 0)
            {
                return Content("Vui lòng không hack ứng dụng.");
            }

            string code_district = model.Villages.First().Substring(0, GetLenMaHCByRole(RoleEnum.District));
            if (model.Villages.Count(x => x.Substring(0, code_district.Length) != code_district) != 0 || !CurrentUser.HasRole(RoleEnum.Admin) && !code_district.StartsWith(CurrentUser.MaHC) ||
                Db.Count<DanhMuc_LoaiDT>(x => Sql.In(x.MaLDT, model.LoaiDTs)) != model.LoaiDTs.Count || Db.Count<DanhMuc_HanhChinh>(x => Sql.In(x.MaHC, model.Villages)) != model.Villages.Count)
            {
                return Content("Vui lòng không hack ứng dụng.");
            }

            string EXPORT_DIR = Server.MapPath(string.Format("~/{0}", ConfigurationManager.AppSettings["EXPORT_DIR"]));
            string EXPORT_TPL_BCDSCTTC = Server.MapPath(string.Format("~/{0}", ConfigurationManager.AppSettings["EXPORT_TPL_BCDSCTTC"]));
            string EXPORT_NAME_BCDSCTTC = "Bao-Cao-Danh-Sach-Chi-Tra-Tro-Cap";

            Guid guid = Guid.NewGuid();
            string word_path = ExportWord_BaoCao_DSChiTraTroCap(model, guid);
            if (string.IsNullOrEmpty(word_path)) { return Content("Thông báo: Không tìm thấy biến động."); }
            byte[] word_bytes = System.IO.File.ReadAllBytes(word_path);
            if (model.Action == "download")
            {
                string file_name = string.Format("{0}.docx", EXPORT_NAME_BCDSCTTC);
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(word_bytes, 0, word_bytes.Length);
                    Response.Clear();
                    Response.AddHeader("Content-Disposition", "attachmnet; filename=" + file_name);
                    Response.ContentType = "application/octet-stream";
                    Response.BinaryWrite(ms.ToArray());
                    Response.Flush();
                    Response.End();
                }
            }
            else if (model.Action == "preview")
            {
                string file_name = string.Format("{0}.html", EXPORT_NAME_BCDSCTTC);
                string dir_path = Path.Combine(EXPORT_DIR, string.Format("{0}_{1}", EXPORT_NAME_BCDSCTTC, guid));
                ConvertToHtml(word_bytes, new DirectoryInfo(dir_path), file_name);
                Response.Redirect(string.Format("/{0}/{1}/{2}", ConfigurationManager.AppSettings["EXPORT_DIR"], string.Format("{0}_{1}", EXPORT_NAME_BCDSCTTC, guid), file_name));
            }
            return Content("Vui lòng không hack ứng dụng.");
        }

        private string ExportWord_BaoCao_DSChiTraTroCap(BaoCao_DSChiTraTroCapModel model, Guid guid)
        {
            #region Initialize
            DateTime report_dt = new DateTime(model.Nam, model.Thang, 1, 0, 0, 0, 0);
            string query = string.Format(@"
            SELECT 
	            DT.HoTen,
	            DT.NgaySinh,
	            DT.ThangSinh,
	            DT.NamSinh,
	            DM_HC.TenHC, 
	            DM_DC.TenDiaChi,
	            DM_LDT.TenLDT,
	            (SELECT MaLDT FROM DanhMuc.DanhMuc_LoaiDT WITH (NOLOCK) WHERE MaLDT = LEFT(BD.MaLDT, 2)) AS MaLDT_Parent, 
	            (SELECT TenLDT FROM DanhMuc.DanhMuc_LoaiDT WITH (NOLOCK) WHERE MaLDT = LEFT(BD.MaLDT, 2)) AS TenLDT_Parent, 
	            BD.*
            FROM 
	            DoiTuong.DoiTuong_BienDong AS BD WITH (NOLOCK) 
	            INNER JOIN DoiTuong.DoiTuong AS DT WITH (NOLOCK) ON BD.IDDT = DT.Id 
	            INNER JOIN DanhMuc.DanhMuc_HanhChinh AS DM_HC WITH (NOLOCK) ON BD.MaHC = DM_HC.MaHC 
	            INNER JOIN DanhMuc.DanhMuc_DiaChi AS DM_DC WITH (NOLOCK) ON BD.IDDiaChi = DM_DC.IDDiaChi 
	            INNER JOIN DanhMuc.DanhMuc_LoaiDT AS DM_LDT WITH (NOLOCK) ON BD.MaLDT = DM_LDT.MaLDT 
            WHERE 
	            BD.Id IN ( 
		            SELECT MAX(Id) 
		            FROM 
			            DoiTuong.DoiTuong_BienDong WITH (NOLOCK) 
		            WHERE 
			            NgayHuong >= '{0:yyyy-MM-dd HH:mm:ss.fff}' 
			            AND NgayHuong < '{1:yyyy-MM-dd HH:mm:ss.fff}' 
		            GROUP BY IDDT) 
	            AND BD.TinhTrang LIKE 'H%' 
	            AND BD.MaHC IN ('{2}') 
	            AND LEFT(BD.MaLDT, 2) IN ('{3}');",
                report_dt, report_dt.AddMonths(1), string.Join("', '", model.Villages), string.Join("', '", model.LoaiDTs));
            #endregion

            #region Retrieve data
            List<DoiTuong_BienDong> lst_biendong = Db.SqlList<DoiTuong_BienDong>(query);
            List<DanhMuc_HanhChinh> lst_village = Db.Select<DanhMuc_HanhChinh>(x => x.Where(y => Sql.In(y.MaHC, model.Villages)).Limit(0, model.Villages.Count));
            List<DanhMuc_LoaiDT> lst_loaidt = Db.Select<DanhMuc_LoaiDT>(x => x.Where(y => Sql.In(y.MaLDT, model.LoaiDTs)).Limit(0, model.LoaiDTs.Count));
            DanhMuc_HanhChinh obj_district = Db.Select<DanhMuc_HanhChinh>(x => x.Where(y => y.MaHC == model.Villages.First().Substring(0, GetLenMaHCByRole(RoleEnum.District))).Limit(0, 1)).First();
            object obj_setting = DataLayer.Models.System.Settings.Get(Enum_Settings_Key.TL01, obj_district.MaHC, null, Enum_Settings_DataType.Raw);
            #endregion

            #region Prepare data
            if (lst_biendong.Count == 0) { return null; }
            string EXPORT_DIR = Server.MapPath(string.Format("~/{0}", ConfigurationManager.AppSettings["EXPORT_DIR"]));
            string EXPORT_TPL_BCDSCTTC = Server.MapPath(string.Format("~/{0}", ConfigurationManager.AppSettings["EXPORT_TPL_BCDSCTTC"]));
            string EXPORT_NAME_BCDSCTTC = "Bao-Cao-Danh-Sach-Chi-Tra-Tro-Cap";

            string path_dir = Path.Combine(EXPORT_DIR, string.Format("{0}_{1}", EXPORT_NAME_BCDSCTTC, guid));
            string path_rpt = Path.Combine(path_dir, string.Format("{0}.docx", EXPORT_NAME_BCDSCTTC));
            Directory.CreateDirectory(path_dir);

            List<Source> lst_src = new List<Source>();
            foreach (DanhMuc_HanhChinh obj_village in lst_village)
            {
                List<DoiTuong_BienDong> lst_biendong_village = lst_biendong.Where(x => x.MaHC == obj_village.MaHC).ToList();
                if (lst_biendong_village.Count == 0) { continue; }
                string path_village = Path.Combine(path_dir, string.Format("{0}_{1}.docx", EXPORT_NAME_BCDSCTTC, obj_village.MaHC));
                System.IO.File.Copy(EXPORT_TPL_BCDSCTTC, path_village);
                using (WordprocessingDocument wDoc = WordprocessingDocument.Open(path_village, true))
                {
                    TextReplacer.SearchAndReplace(wDoc, "{$Thang}", model.Thang.ToString(), true);
                    TextReplacer.SearchAndReplace(wDoc, "{$Nam}", model.Nam.ToString(), true);
                    TextReplacer.SearchAndReplace(wDoc, "{$MaHC_District}", obj_district.MaHC, true);
                    TextReplacer.SearchAndReplace(wDoc, "{$TenHC_District}", obj_district.TenHC, true);
                    TextReplacer.SearchAndReplace(wDoc, "{$TenHC_Village}", obj_village.TenHC, true);
                    TextReplacer.SearchAndReplace(wDoc, "{$SoNguoi}", lst_biendong_village.Count.ToString(), true);
                    TextReplacer.SearchAndReplace(wDoc, "{$SoTien_So}", lst_biendong_village.Sum(x => x.MucTC.Value).ToString("#,000"), true);
                    TextReplacer.SearchAndReplace(wDoc, "{$SoTien_Chu}", ((long)lst_biendong_village.Sum(x => x.MucTC.Value)).ToString().GetCurrencyVNText(), true);
                    TextReplacer.SearchAndReplace(wDoc, "{$NguoiKy}", obj_setting != null ? ((PhotoBookmart.DataLayer.Models.System.Settings)obj_setting).Value : " ", true);
                    Table wTable = wDoc.MainDocumentPart.Document.Body.Elements<Table>().First();
                    foreach(DanhMuc_LoaiDT obj_loaidt in lst_loaidt)
                    {
                        List<DoiTuong_BienDong> lst_biendong_loaidt = lst_biendong_village.Where(x => x.MaLDT.StartsWith(obj_loaidt.MaLDT)).ToList();
                        if (lst_biendong_loaidt.Count == 0) { continue; }
                        wTable.Append(GenerateTableRowLoaiI(obj_loaidt.TenLDT, lst_biendong_loaidt.Sum(x => x.MucTC.Value)));
                        foreach(var obj_biendong_group in lst_biendong_loaidt.GroupBy(x => new { x.MaLDT }))
                        {
                            wTable.Append(GenerateTableRowLoaiII(obj_biendong_group.First().TenLDT, obj_biendong_group.Sum(x => x.MucTC.Value)));
                            for(int i = 0; i < obj_biendong_group.Count(); i++)
                            {
                                wTable.Append(GenerateTableRowBienDong(obj_biendong_group.ElementAt(i), i + 1));
                            }
                        }
                    }
                    wDoc.MainDocumentPart.Document.Save();
                }
                lst_src.Add(new Source(new WmlDocument(path_village), true));
            }
            DocumentBuilder.BuildDocument(lst_src, path_rpt);
            return path_rpt;
            #endregion
        }

        private void ConvertToHtml(byte[] byteArray, DirectoryInfo desDirectory, string htmlFileName)
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
                            System.Drawing.Imaging.ImageFormat imageFormat = null;
                            if (extensions == "png")
                            {
                                extensions = "gif";
                                imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            }
                            else if (extensions == "gif")
                            {
                                imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            }
                            else if (extensions == "bmp")
                            {
                                imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            }
                            else if (extensions == "jpeg")
                            {
                                imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            }
                            else if (extensions == "tiff")
                            {
                                extensions = "gif";
                                imageFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            }
                            else if (extensions == "wmf")
                            {
                                extensions = "wmf";
                                imageFormat = System.Drawing.Imaging.ImageFormat.Wmf;
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
                        new XElement(Xhtml.style,
                            "body { width:756px; margin:auto; }"));
                    /*body.AddFirst(
                        new XElement(Xhtml.p,
                            new XElement(Xhtml.A,
                                new XAttribute("href", "/WebForm1.aspx"), "Go back to Upload Page")));*/

                    var htmlString = html.ToString(SaveOptions.DisableFormatting);
                    System.IO.File.WriteAllText(fiHtml.FullName, htmlString, Encoding.UTF8);
                }
            }
        }

        public ActionResult List(OrderReportFilterModel model)
        {
            List<Order> c = new List<Order>();
            List<OrderReportModel> d = new List<OrderReportModel>();

            var p = PredicateBuilder.True<Order>();
            p = p.And(x => x.Payment_isPaid);
            if (model.FromDate.HasValue)
            {

                p = p.And(m => m.CreatedOn >= model.FromDate.Value);
            }
            if (model.ToDate.HasValue)
            {

                p = p.And(m => m.CreatedOn <= model.ToDate.Value);
            }

            c = Db.Select<Order>().OrderBy(x => (x.CreatedOn)).ToList();
            if (c.Count != 0)
            {
                DateTime dtFrom = DateTime.ParseExact(c.First().CreatedOn.ToString("dd/MM/yyyy"), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime dtTo = dtFrom.AddHours(23).AddMinutes(59).AddSeconds(59);
                do
                {
                    List<Order> lstOrderTrunc = c.Where(x => (x.CreatedOn >= dtFrom && x.CreatedOn <= dtTo)).OrderBy(x => x.Shipping_DisplayPriceSign).ToList();
                    if (lstOrderTrunc.Count != 0)
                    {
                        string priceSign = lstOrderTrunc.First().Shipping_DisplayPriceSign;
                        int numOrders = 0;
                        double totalMoney = 0;
                        foreach (Order order in lstOrderTrunc)
                        {
                            if (order.Shipping_DisplayPriceSign == priceSign)
                            {
                                numOrders++;
                                totalMoney += order.Bill_Total;
                                continue;
                            }
                            d.Add(new OrderReportModel()
                            {
                                FromDate = dtFrom,
                                ToDate = dtTo,
                                PriceSign = priceSign,
                                NumOrders = numOrders,
                                TotalMoney = totalMoney
                            });
                            priceSign = order.Shipping_DisplayPriceSign;
                            numOrders = 1;
                            totalMoney = order.Bill_Total;
                        }
                        d.Add(new OrderReportModel()
                        {
                            FromDate = dtFrom,
                            ToDate = dtTo,
                            PriceSign = priceSign,
                            NumOrders = numOrders,
                            TotalMoney = totalMoney
                        });
                    }
                    dtFrom = dtFrom.Add(TimeSpan.FromDays(1));
                    dtTo = dtTo.Add(TimeSpan.FromDays(1));
                }
                while (c.Last().CreatedOn >= dtFrom);
            }

            return PartialView("_List", d);
        }

        #region Support OpenXML
        public DocumentFormat.OpenXml.Wordprocessing.TableRow GenerateTableRowLoaiI(string name, decimal money)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableRow tableRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            DocumentFormat.OpenXml.Wordprocessing.TableCell[] tableCell = new DocumentFormat.OpenXml.Wordprocessing.TableCell[3] { new DocumentFormat.OpenXml.Wordprocessing.TableCell(), new DocumentFormat.OpenXml.Wordprocessing.TableCell(), new DocumentFormat.OpenXml.Wordprocessing.TableCell() };
            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties[] tableCellProperties = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties[3] { new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(), new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(), new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties() };
            DocumentFormat.OpenXml.Wordprocessing.TableCellWidth[] tableCellWidth = new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth[3] { new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth(), new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth(), new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth() };
            DocumentFormat.OpenXml.Wordprocessing.GridSpan[] gridSpan = new DocumentFormat.OpenXml.Wordprocessing.GridSpan[3] { new DocumentFormat.OpenXml.Wordprocessing.GridSpan(), new DocumentFormat.OpenXml.Wordprocessing.GridSpan(), new DocumentFormat.OpenXml.Wordprocessing.GridSpan() };
            DocumentFormat.OpenXml.Wordprocessing.Paragraph[] paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph[3] { new DocumentFormat.OpenXml.Wordprocessing.Paragraph(), new DocumentFormat.OpenXml.Wordprocessing.Paragraph(), new DocumentFormat.OpenXml.Wordprocessing.Paragraph() };
            DocumentFormat.OpenXml.Wordprocessing.Run[] run = new DocumentFormat.OpenXml.Wordprocessing.Run[3] { new DocumentFormat.OpenXml.Wordprocessing.Run(), new DocumentFormat.OpenXml.Wordprocessing.Run(), new DocumentFormat.OpenXml.Wordprocessing.Run() };
            DocumentFormat.OpenXml.Wordprocessing.Text[] text= new DocumentFormat.OpenXml.Wordprocessing.Text[3] { new DocumentFormat.OpenXml.Wordprocessing.Text(), new DocumentFormat.OpenXml.Wordprocessing.Text(), new DocumentFormat.OpenXml.Wordprocessing.Text() };
            
            #region Cell #1
            tableCellWidth[0].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Auto;
            gridSpan[0].Val = 4;
            tableCellProperties[0].Append(tableCellWidth[0]);
            tableCellProperties[0].Append(gridSpan[0]);
            
            text[0] = new DocumentFormat.OpenXml.Wordprocessing.Text(name);
            run[0].Append(text[0]);
            paragraph[0].Append(run[0]);

            tableCell[0].Append(tableCellProperties[0]);
            tableCell[0].Append(paragraph[0]);
            tableRow.Append(tableCell[0]);
            #endregion

            #region Cell #2
            tableCellWidth[1].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Auto;
            gridSpan[1].Val = 1;
            tableCellProperties[1].Append(tableCellWidth[1]);
            tableCellProperties[1].Append(gridSpan[1]);

            text[1] = new DocumentFormat.OpenXml.Wordprocessing.Text(money.ToString("#,000"));
            run[1].Append(text[1]);
            paragraph[1].Append(run[1]);

            tableCell[1].Append(tableCellProperties[1]);
            tableCell[1].Append(paragraph[1]);
            tableRow.Append(tableCell[1]);
            #endregion

            #region Cell #3
            tableCellWidth[2].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Auto;
            gridSpan[2].Val = 1;
            tableCellProperties[2].Append(tableCellWidth[2]);
            tableCellProperties[2].Append(gridSpan[2]);

            text[2] = new DocumentFormat.OpenXml.Wordprocessing.Text(null);
            run[2].Append(text[2]);
            paragraph[2].Append(run[2]);

            tableCell[2].Append(tableCellProperties[2]);
            tableCell[2].Append(paragraph[2]);
            tableRow.Append(tableCell[2]);
            #endregion

            return tableRow;
        }

        public DocumentFormat.OpenXml.Wordprocessing.TableRow GenerateTableRowLoaiII(string name, decimal money)
        {
            DocumentFormat.OpenXml.Wordprocessing.TableRow tableRow = new DocumentFormat.OpenXml.Wordprocessing.TableRow();
            DocumentFormat.OpenXml.Wordprocessing.TableCell[] tableCell = new DocumentFormat.OpenXml.Wordprocessing.TableCell[4] { new DocumentFormat.OpenXml.Wordprocessing.TableCell(), new DocumentFormat.OpenXml.Wordprocessing.TableCell(), new DocumentFormat.OpenXml.Wordprocessing.TableCell(), new DocumentFormat.OpenXml.Wordprocessing.TableCell() };
            DocumentFormat.OpenXml.Wordprocessing.TableCellProperties[] tableCellProperties = new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties[4] { new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(), new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(), new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties(), new DocumentFormat.OpenXml.Wordprocessing.TableCellProperties() };
            DocumentFormat.OpenXml.Wordprocessing.TableCellWidth[] tableCellWidth = new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth[4] { new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth(), new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth(), new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth(), new DocumentFormat.OpenXml.Wordprocessing.TableCellWidth() };
            DocumentFormat.OpenXml.Wordprocessing.GridSpan[] gridSpan = new DocumentFormat.OpenXml.Wordprocessing.GridSpan[4] { new DocumentFormat.OpenXml.Wordprocessing.GridSpan(), new DocumentFormat.OpenXml.Wordprocessing.GridSpan(), new DocumentFormat.OpenXml.Wordprocessing.GridSpan(), new DocumentFormat.OpenXml.Wordprocessing.GridSpan() };
            DocumentFormat.OpenXml.Wordprocessing.Paragraph[] paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph[4] { new DocumentFormat.OpenXml.Wordprocessing.Paragraph(), new DocumentFormat.OpenXml.Wordprocessing.Paragraph(), new DocumentFormat.OpenXml.Wordprocessing.Paragraph(), new DocumentFormat.OpenXml.Wordprocessing.Paragraph() };
            DocumentFormat.OpenXml.Wordprocessing.Run[] run = new DocumentFormat.OpenXml.Wordprocessing.Run[4] { new DocumentFormat.OpenXml.Wordprocessing.Run(), new DocumentFormat.OpenXml.Wordprocessing.Run(), new DocumentFormat.OpenXml.Wordprocessing.Run(), new DocumentFormat.OpenXml.Wordprocessing.Run() };
            DocumentFormat.OpenXml.Wordprocessing.Text[] text = new DocumentFormat.OpenXml.Wordprocessing.Text[4] { new DocumentFormat.OpenXml.Wordprocessing.Text(), new DocumentFormat.OpenXml.Wordprocessing.Text(), new DocumentFormat.OpenXml.Wordprocessing.Text(), new DocumentFormat.OpenXml.Wordprocessing.Text() };

            #region Cell #1
            tableCellWidth[0].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa;
            tableCellWidth[0].Width = "100";
            gridSpan[0].Val = 1;
            tableCellProperties[0].Append(tableCellWidth[0]);
            tableCellProperties[0].Append(gridSpan[0]);

            text[0] = new DocumentFormat.OpenXml.Wordprocessing.Text(null);
            run[0].Append(text[0]);
            paragraph[0].Append(run[0]);

            tableCell[0].Append(tableCellProperties[0]);
            tableCell[0].Append(paragraph[0]);
            tableRow.Append(tableCell[0]);
            #endregion

            #region Cell #2
            tableCellWidth[1].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa;
            tableCellWidth[1].Width = "300";
            gridSpan[1].Val = 3;
            tableCellProperties[1].Append(tableCellWidth[1]);
            tableCellProperties[1].Append(gridSpan[1]);

            text[1] = new DocumentFormat.OpenXml.Wordprocessing.Text(name);
            run[1].Append(text[1]);
            paragraph[1].Append(run[1]);

            tableCell[1].Append(tableCellProperties[1]);
            tableCell[1].Append(paragraph[1]);
            tableRow.Append(tableCell[1]);
            #endregion

            #region Cell #3
            tableCellWidth[2].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa;
            tableCellWidth[2].Width = "100";
            gridSpan[2].Val = 1;
            tableCellProperties[2].Append(tableCellWidth[2]);
            tableCellProperties[2].Append(gridSpan[2]);

            text[2] = new DocumentFormat.OpenXml.Wordprocessing.Text(money.ToString("#,000"));
            run[2].Append(text[2]);
            paragraph[2].Append(run[2]);

            tableCell[2].Append(tableCellProperties[2]);
            tableCell[2].Append(paragraph[2]);
            tableRow.Append(tableCell[2]);
            #endregion

            #region Cell #4
            tableCellWidth[3].Type = DocumentFormat.OpenXml.Wordprocessing.TableWidthUnitValues.Dxa;
            tableCellWidth[3].Width = "100";
            gridSpan[3].Val = 1;
            tableCellProperties[3].Append(tableCellWidth[3]);
            tableCellProperties[3].Append(gridSpan[3]);

            text[3] = new DocumentFormat.OpenXml.Wordprocessing.Text(null);
            run[3].Append(text[3]);
            paragraph[3].Append(run[3]);

            tableCell[3].Append(tableCellProperties[3]);
            tableCell[3].Append(paragraph[3]);
            tableRow.Append(tableCell[3]);
            #endregion

            return tableRow;
        }
        
        public DocumentFormat.OpenXml.Wordprocessing.TableRow GenerateTableRowBienDong(DoiTuong_BienDong bien_dong, int no)
        {
            TableRow tableRow = new TableRow();
            TableCell[] tableCell = new TableCell[6] { new TableCell(), new TableCell(), new TableCell(), new TableCell(), new TableCell(), new TableCell() };
            TableCellProperties[] tableCellProperties = new TableCellProperties[6] { new TableCellProperties(), new TableCellProperties(), new TableCellProperties(), new TableCellProperties(), new TableCellProperties(), new TableCellProperties() };
            TableCellWidth[] tableCellWidth = new TableCellWidth[6] { new TableCellWidth(), new TableCellWidth(), new TableCellWidth(), new TableCellWidth(), new TableCellWidth(), new TableCellWidth() };
            GridSpan[] gridSpan = new GridSpan[6] { new GridSpan(), new GridSpan(), new GridSpan(), new GridSpan(), new GridSpan(), new GridSpan() };
            Paragraph[] paragraph = new Paragraph[6] { new Paragraph(), new Paragraph(), new Paragraph(), new Paragraph(), new Paragraph(), new Paragraph() };
            Run[] run = new Run[6] { new Run(), new Run(), new Run(), new Run(), new Run(), new Run() };
            Text[] text = new Text[6] { new Text(), new Text(), new Text(), new Text(), new Text(), new Text() };

            #region Cell #1
            tableCellWidth[0].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[0].Width = "100";
            gridSpan[0].Val = 1;
            tableCellProperties[0].Append(tableCellWidth[0]);
            tableCellProperties[0].Append(gridSpan[0]);

            text[0] = new Text(no.ToString());
            run[0].Append(text[0]);
            paragraph[0].Append(run[0]);

            tableCell[0].Append(tableCellProperties[0]);
            tableCell[0].Append(paragraph[0]);
            tableRow.Append(tableCell[0]);
            #endregion

            #region Cell #2
            tableCellWidth[1].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[1].Width = "100";
            gridSpan[1].Val = 1;
            tableCellProperties[1].Append(tableCellWidth[1]);
            tableCellProperties[1].Append(gridSpan[1]);

            text[1] = new Text(bien_dong.HoTen);
            run[1].Append(text[1]);
            paragraph[1].Append(run[1]);

            tableCell[1].Append(tableCellProperties[1]);
            tableCell[1].Append(paragraph[1]);
            tableRow.Append(tableCell[1]);
            #endregion

            #region Cell #3
            tableCellWidth[2].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[2].Width = "100";
            gridSpan[2].Val = 1;
            tableCellProperties[2].Append(tableCellWidth[2]);
            tableCellProperties[2].Append(gridSpan[2]);

            text[2] = new Text(bien_dong.NamSinh.GetDateOfBirth(bien_dong.ThangSinh, bien_dong.NgaySinh));
            run[2].Append(text[2]);
            paragraph[2].Append(run[2]);

            tableCell[2].Append(tableCellProperties[2]);
            tableCell[2].Append(paragraph[2]);
            tableRow.Append(tableCell[2]);
            #endregion

            #region Cell #4
            tableCellWidth[3].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[3].Width = "100";
            gridSpan[3].Val = 1;
            tableCellProperties[3].Append(tableCellWidth[3]);
            tableCellProperties[3].Append(gridSpan[3]);

            text[3] = new Text(bien_dong.TenDiaChi);
            run[3].Append(text[3]);
            paragraph[3].Append(run[3]);

            tableCell[3].Append(tableCellProperties[3]);
            tableCell[3].Append(paragraph[3]);
            tableRow.Append(tableCell[3]);
            #endregion

            #region Cell #5
            tableCellWidth[4].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[4].Width = "100";
            gridSpan[4].Val = 1;
            tableCellProperties[4].Append(tableCellWidth[4]);
            tableCellProperties[4].Append(gridSpan[4]);

            text[4] = new Text(bien_dong.MucTC.Value.ToString("#,000"));
            run[4].Append(text[4]);
            paragraph[4].Append(run[4]);

            tableCell[4].Append(tableCellProperties[4]);
            tableCell[4].Append(paragraph[4]);
            tableRow.Append(tableCell[4]);
            #endregion

            #region Cell #6
            tableCellWidth[5].Type = TableWidthUnitValues.Dxa;
            tableCellWidth[5].Width = "100";
            gridSpan[5].Val = 1;
            tableCellProperties[5].Append(tableCellWidth[5]);
            tableCellProperties[5].Append(gridSpan[5]);

            text[5] = new Text(null);
            run[5].Append(text[5]);
            paragraph[5].Append(run[5]);

            tableCell[5].Append(tableCellProperties[5]);
            tableCell[5].Append(paragraph[5]);
            tableRow.Append(tableCell[5]);
            #endregion

            return tableRow;
        }
        #endregion
    }
}
