using System;
using System.Collections.Generic;
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

            string word_path = ExportWord_BaoCao_DSChiTraTroCap(model);
            byte[] word_bytes = System.IO.File.ReadAllBytes(word_path);
            string file_name = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(word_path), DateTime.Now.ToFileTime());
            if (model.Action == "download")
            {
                file_name = string.Format("{0}.docx", file_name);
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
                file_name = string.Format("{0}.html", file_name);
                ConvertToHtml(word_bytes, new DirectoryInfo(Server.MapPath("~/Reports")), file_name);
                Response.Redirect(string.Format("/Reports/{0}", file_name));
            }
            return Content("Vui lòng không hack ứng dụng.");
        }

        private string ExportWord_BaoCao_DSChiTraTroCap(BaoCao_DSChiTraTroCapModel model)
        {
            string path_output = Server.MapPath("~/Reports/BienDong.docx");
            return path_output;
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
    }
}
