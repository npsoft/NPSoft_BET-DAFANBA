using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoBookmart.Areas.Administration.Models;
using PhotoBookmart.Common.Helpers;
using System.IO;
using PhotoBookmart.Controllers;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PhotoBookmart.Areas.Administration.Controllers;
using PhotoBookmart.DataLayer.Models.System;
using PhotoBookmart.DataLayer.Models.Users_Management;
using PhotoBookmart.DataLayer.Models.Sites;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.Support;

namespace PhotoBookmart.Areas.Administration.Controllers
{
    [ABRequiresAnyRole(RoleEnum.Administrator)]
    public class WebsiteProductController : WebAdminController
    {
        #region Product List
        public ActionResult Index(int? cat_id)
        {
            Product_Category cat = new Product_Category();
            cat.Id = 0;
            if (cat_id.HasValue)
            {
                cat = Cache_GetProductCategory().Where(m => m.Id == cat_id.Value).FirstOrDefault();
                if (cat == null)
                {
                    return RedirectToAction("Index", "Management");
                }
            }

            ViewData["Cat"] = cat;

            return View();
        }

        public ActionResult List(int cat_id)
        {
            List<Product> c = new List<Product>();

            JoinSqlBuilder<Product, Product> jn = new JoinSqlBuilder<Product, Product>();

            if (cat_id > 0)
            {
                //jn = jn.Join<Product, Product_In_Category>(m => m.Id, k => k.ProductId);

                jn = jn.Where<Product>(m => m.CatId == cat_id);
            }
            jn = jn.OrderBy<Product>(x => x.Order);
            var sql = jn.ToSql();

            c = Db.Select<Product>(sql);

            var list_users = Cache_GetAllUsers();

            var cats = Db.Select<Product_Category>();

            foreach (var x in c)
            {
                var z = list_users.Where(m => m.Id == x.CreatedBy);
                if (z.Count() > 0)
                {
                    var k = z.First();
                    if (string.IsNullOrEmpty(k.FullName))
                        x.CreatedByUsername = k.UserName;
                    else
                        x.CreatedByUsername = k.FullName;
                }
                else
                {
                    x.CreatedByUsername = "Deleted user";
                }

                var kk = cats.Where(q => q.Id == x.CatId).FirstOrDefault();
                if (kk != null)
                {
                    x.Category_Name = kk.Name;
                }
                else
                {
                    x.Category_Name = "Deleted Category";
                }
            }
            return PartialView("_List", c);
        }

        public ActionResult Add(int? cat_id)
        {
            Product model = new Product();

            if (!cat_id.HasValue || cat_id <= 0)
            {
                cat_id = 1;
            }

            var cat = Cache_GetProductCategory().Where(m => m.Id == cat_id.Value).FirstOrDefault();
            // get default category
            if (cat == null)
            {
                cat = Cache_GetProductCategory().Where(m => m.Id == 1).FirstOrDefault();
            }

            if (cat != null)
            {
                model.CatId = cat.Id;
                model.Category_Name = cat.Name;
            }


            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var models = Db.Where<Product>(m => m.Id == id);
            if (models.Count == 0)
            {
                return RedirectToAction("Index", "Management");
            }
            else
            {
                var model = models.First();

                //var site = Cache_GetAllWebsite().Where(m => m.Id == model.WebsiteId).FirstOrDefault();
                return View("Add", model);
            }
        }

        [ValidateInput(false)]
        public ActionResult Update(Product model, IEnumerable<HttpPostedFileBase> FileUp, string Status)
        {
            model.Status = Status != null ? true : false;

            if (string.IsNullOrEmpty(model.Name))
            {
                return JsonError("Please enter field » Name");
            }

            if (model.Weight < 0)
            {
                model.Weight = 0;
            }

            // generate seo name
            string random = "";
            do
            {

                if (string.IsNullOrEmpty(model.SeoName))
                {
                    model.SeoName = model.Name + random;
                    model.SeoName = model.SeoName.ToSeoUrl();
                }
                else
                {
                    model.SeoName = model.SeoName.ToSeoUrl();
                }

                // check exist
                if (Db.Count<Product>(m => m.SeoName == model.SeoName && m.Id != model.Id) == 0)
                {
                    break;
                }

                random = "_" + random.GenerateRandomText(3);
                model.SeoName = "";
            } while (0 < 1);

            Product current_item = new Product();
            if (model.Id > 0)
            {
                var z = Db.Where<Product>(m => m.Id == model.Id);
                if (z.Count == 0)
                {
                    // the ID is not exist
                    return JsonError("Please dont try to hack us");
                }
                else
                {
                    current_item = z.First();
                }
            }

            if (model.Id == 0)
            {
                model.CreatedOn = DateTime.Now;
                model.CreatedBy = AuthenticatedUserID;

            }
            else
            {
                model.CreatedOn = current_item.CreatedOn;
                model.CreatedBy = current_item.CreatedBy;
            }

            if (model.Id == 0 || current_item.Order == 0)
            {
                // set Order Menu
                try
                {
                    int OrderMenu = Db.Where<Product>(m => m.CatId == model.CatId).Max(m => m.Order);
                    model.Order = OrderMenu + 1;
                }
                catch
                {
                    model.Order = 0;
                }
            }
            else
            {
                model.Order = current_item.Order;
            }

            if (model.Id == 0)
            {
                Db.Insert<Product>(model);

                //// insert into category
                //if (model.CatId > 0)
                //{
                //    var p = new Product_In_Category() { Category_Id = model.CatId, CreatedBy = AuthenticatedUserID, CreatedOn = DateTime.Now, ProductId = (int)Db.GetLastInsertId() };
                //    Db.Insert<Product_In_Category>(p);
                //}
            }
            else
            {
                Db.Update<Product>(model);
            }

            return JsonSuccess(Url.Action("Index", new { cat_id = model.CatId }));
        }

        public ActionResult Delete(int id)
        {
            try
            {
                if (id > 1)
                {
                    Db.DeleteById<Product>(id);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(int id)
        {
            var model = Db.Where<Product>(m => m.Id == id).FirstOrDefault();
            if (model == null)
                return Redirect("/");


            // created by username
            var list_users = Cache_GetAllUsers();

            var zk = list_users.Where(m => m.Id == model.CreatedBy).FirstOrDefault();
            if (zk == null)
            {
                model.CreatedByUsername = "Deleted User";
            }
            else
            {
                if (string.IsNullOrEmpty(zk.FullName))
                    model.CreatedByUsername = zk.UserName;
                else
                    model.CreatedByUsername = zk.FullName;
            }

            return View(model);
        }

        public ActionResult Move(long id, int move)
        {
            try
            {
                var e = Db.SelectParam<Product>(m => (m.Id == id)).FirstOrDefault();

                var a = new List<Product>();

                var t = new Product();

                if (move == 1)
                {
                    a = Db.Where<Product>(m => (m.CatId == e.CatId && m.Order < e.Order)).OrderBy(m => (m.Order)).ToList();

                    if (a.Count != 0) t = a.LastOrDefault();
                }
                else
                {
                    a = Db.Where<Product>(m => (m.CatId == e.CatId && m.Order > e.Order)).OrderBy(m => (m.Order)).ToList();

                    if (a.Count != 0) t = a.FirstOrDefault();
                }

                if (t.Id > 0)
                {
                    int i = t.Order;

                    t.Order = e.Order;

                    e.Order = i;

                    Db.Update<Product>(t);

                    Db.Update<Product>(e);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Detail Option in Product

        /// <param name="id">Site ID</param>
        /// <returns></returns>
        public ActionResult Detail_Option_List(int id)
        {
            var c = Db.Where<OptionInProduct>(m => m.ProductId == id);

            // created by username
            var list_users = Cache_GetAllUsers();
            var options = Db.Select<Product_Option>();

            foreach (var x in c)
            {
                var z = list_users.Where(m => m.Id == x.CreatedBy);
                if (z.Count() > 0)
                {
                    var k = z.First();
                    if (string.IsNullOrEmpty(k.FullName))
                        x.CreatedByUsername = k.UserName;
                    else
                        x.CreatedByUsername = k.FullName;
                }
                else
                {
                    x.CreatedByUsername = "Deleted user";
                }

                var sk = options.Where(m => m.Id == x.ProductOptionId).FirstOrDefault();
                if (sk != null)
                {
                    x.Option_Name = sk.InternalName;
                }
            }

            return PartialView(c);
        }

        public ActionResult Detail_Option_Add(long product_id)
        {
            // check product exist
            var product = Db.Select<Product>(x => x.Where(m => m.Id == product_id).Limit(1)).FirstOrDefault();
            if (product == null)
            {
                return Redirect("/");
            }

            var model = new OptionInProduct();
            model.ProductId = product.Id;
            model.Product_Name = product.Name;

            return View(model);
        }

        public ActionResult Detail_Option_Edit(long id)
        {
            var model = Db.Select<OptionInProduct>(x => x.Where(m => m.Id == id).Limit(1)).FirstOrDefault();
            if (model == null)
            {
                return Redirect("/");
            }

            // get product name and option name 
            var product = Db.Select<Product>(x => x.Where(m => m.Id == model.ProductId).Limit(1)).FirstOrDefault();
            if (product == null)
            {
                model.Product_Name = "Deleted product";
            }
            else
            {
                model.Product_Name = product.Name;
            }

            var option = Db.Select<Product_Option>(x => x.Where(m => m.Id == model.ProductOptionId).Limit(1)).FirstOrDefault();
            if (option == null)
            {
                model.Option_Name = "Deleted option";
            }
            else
            {
                model.Option_Name = option.Name;
            }

            return View("Detail_Option_Add", model);
        }


        public ActionResult Detail_Option_Update(OptionInProduct model)
        {
            var curent_item = new OptionInProduct();
            if (model.Id > 0)
            {
                curent_item = Db.Where<OptionInProduct>(m => m.Id == model.Id).FirstOrDefault();
                if (curent_item == null)
                {
                    return Redirect("/");
                }
            }
            else
            {
                // if we add new, make sure no dupplication
                var x = Db.Where<OptionInProduct>(m => m.ProductOptionId == model.ProductOptionId && m.ProductId == model.ProductId);
                if (x.Count > 0)
                {
                    return JsonError("Duplicated option.");
                }
                curent_item.CreatedBy = AuthenticatedUserID;
                curent_item.CreatedOn = DateTime.Now;
            }

            curent_item.ProductId = model.ProductId;
            curent_item.ProductOptionId = model.ProductOptionId;

            curent_item.isRequire = model.isRequire;
            curent_item.DefaultQuantity = model.DefaultQuantity;
            curent_item.MaxQuantity = model.MaxQuantity;
            curent_item.MinQuantity = model.MinQuantity;
            curent_item.CanApplyCoupon = model.CanApplyCoupon;

            if (model.Id > 0)
            {
                Db.Update<OptionInProduct>(curent_item);
            }
            else
            {
                Db.Insert<OptionInProduct>(curent_item);
            }
            return JsonSuccess(Url.Action("Detail", new { id = curent_item.ProductId }));
        }

        public ActionResult Detail_Option_Delete(int id)
        {
            try
            {
                Db.DeleteById<OptionInProduct>(id);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Detail Image

        /// <param name="id">Site ID</param>
        /// <returns></returns>
        public ActionResult Detail_Image_List(int id)
        {
            var c = Db.Where<Product_Images>(m => m.ProductId == id);

            // created by username
            var list_users = Cache_GetAllUsers();
            var list_cat = Cache_GetProductCategory();

            foreach (var x in c)
            {
                var z = list_users.Where(m => m.Id == x.CreatedBy);
                if (z.Count() > 0)
                {
                    var k = z.First();
                    if (string.IsNullOrEmpty(k.FullName))
                        x.CreatedByUsername = k.UserName;
                    else
                        x.CreatedByUsername = k.FullName;
                }
                else
                {
                    x.CreatedByUsername = "Deleted user";
                }
            }

            return PartialView(c);
        }

        public ActionResult Detail_Image_Add(Product_Images model, IEnumerable<HttpPostedFileBase> FileUp)
        {
            var curent_item = new Product_Images();
            if (model.Id > 0)
            {
                curent_item = Db.Where<Product_Images>(m => m.Id == model.Id).FirstOrDefault();
                if (curent_item == null)
                {
                    return JsonError("Please dont try to hack us");
                }
            }
            else
            {
                curent_item.CreatedBy = AuthenticatedUserID;
                curent_item.CreatedOn = DateTime.Now;
            }

            curent_item.ProductId = model.ProductId;
            curent_item.Name = model.Name;
            curent_item.Status = model.Status;

            if (FileUp != null && FileUp.FirstOrDefault() != null)
            {
                curent_item.Filename = UploadFile(AuthenticatedUserID, model.ProductId.ToString(), "ProductImage", FileUp);
            }

            if (model.Id > 0)
            {
                Db.Update<Product_Images>(curent_item);
            }
            else
            {
                Db.Insert<Product_Images>(curent_item);
            }
            return RedirectToAction("Detail", new { id = model.ProductId });
        }

        public ActionResult Detail_Image_Delete(int id)
        {
            try
            {
                var x = Db.Where<Product_Images>(m => m.Id == id).FirstOrDefault();
                if (x != null)
                {
                    var path = Server.MapPath("~/" + x.Filename);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                Db.DeleteById<Product_Images>(id);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Support

        public ActionResult ExportExcel()
        {
            return ExportListProduct();
        }

        private ActionResult ExportListProduct()
        {
            var package = new ExcelPackage();

            package.Workbook.Worksheets.Add("Products");
            ExcelWorksheet ws = package.Workbook.Worksheets[1];
            ws.Name = "Products"; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 12; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            //Merging cells and create a center heading for out table
            ws.Cells[1, 1].Value = "List of Photobookmart Products "; // Heading Name
            ws.Cells[1, 1].Style.Font.Size = 22;
            ws.Cells[1, 1, 1, 10].Merge = true; //Merge columns start and end range
            ws.Cells[1, 1, 1, 10].Style.Font.Bold = true; //Font should be bold
            ws.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center


            var lstProductCat = Db.Select<Product_Category>(x => x.Where(y => (y.Status)));

            var lstProductOption = Db.Select<Product_Option>(x => x.Where(y => (y.Status)));


            List<string> headers = new List<string>() { "", "Name", "Size", "Pages", "Price", "Shipping", "PhotoCreation_Id" };

            headers.AddRange(lstProductOption.Select(x => (x.InternalName)).ToArray<string>());

            int row = 3;

            for (int i = 0; i < headers.Count; i++)
            {
                ws.Cells[row, i + 1].Value = headers[i];

                ws.Cells[row, i + 1].Style.Font.Bold = true;

                ws.Cells[row, i + 1].Style.Font.Size = 13;
            }
            //

            FillAllExcel(ref ws, lstProductCat, lstProductOption, headers.Count);


            for (int i = 0; i < headers.Count; i++)
            {
                if (i < headers.Count - lstProductOption.Count)
                {
                    ws.Column(i + 1).AutoFit();
                }
                else
                {
                    ws.Column(i + 1).Width = 25;
                }
            }

            // footer

            ws.View.FreezePanes(3, 7);

            var memoryStream = package.GetAsByteArray();
            package.Dispose();
            var fileName = string.Format("List product {0:yyyy-MM-dd-HH-mm-ss}.xlsx", DateTime.Now);
            package.Dispose();
            return base.File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }

        private void FillAllExcel(ref ExcelWorksheet ws, List<Product_Category> lstProductCat, List<Product_Option> lstProductOption, int header_count)
        {
            var row = 4;

            foreach (var pc in lstProductCat ?? Enumerable.Empty<Product_Category>())
            {
                InitRowCategory(ref ws, pc, row++, header_count);

                var lstProduct = Db.Select<Product>(x => x.Where(y => (y.Status && y.CatId == pc.Id)).OrderBy(z => (z.Order)));
                int _p_index = 1;
                foreach (var p in lstProduct ?? Enumerable.Empty<Product>())
                {
                    InitRowProduct(ref ws, p, row++, lstProductOption, _p_index);
                    _p_index++;
                }

                // footer
                //row++;
                ws.Cells[row, 2].Value = "Total";
                ws.Cells[row, 2].Style.Font.Bold = true;
                ws.Cells[row, 2].Style.Font.Italic = true;
                ws.Cells[row, 2].Style.Font.Size = 11;
                ws.Cells[row, 2, row, 3].Merge = true; //Merge columns start and end range

                ws.Cells[row, 4].Value = lstProduct == null ? 0 : lstProduct.Count;
                row++; row++;
            }


        }

        private void InitRowCategory(ref ExcelWorksheet ws, Product_Category cat, int row, int numOfCols)
        {
            ws.Cells[row, 1].Value = cat.Name;

            ws.Cells[row, 1].Style.Font.Bold = true;

            ws.Cells[row, 1].Style.Font.Size = 11;

            ws.Cells[row, 1, row, numOfCols].Merge = true;
            ws.Cells[row, 1, row, numOfCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, numOfCols].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(230, 232, 235));
        }

        private void InitRowProduct(ref ExcelWorksheet ws, Product product, int row, List<Product_Option> lstProductOption, int product_index)
        {
            var rate = Setting_GetExchangeRate();
            ws.Cells[row, 1].Value = product_index;
            ws.Cells[row, 2].Value = product.Name;

            ws.Cells[row, 3].Value = product.Size;

            ws.Cells[row, 4].Value = string.Format("{0} Pages", product.Pages);

            ws.Cells[row, 5].Value = product.getPrice(Enum_Price_MasterType.Product, rate.Code).Value.ToMoneyFormated(rate.CurrencyCode);

            ws.Cells[row, 6].Value = product.isFreeShip ? "Free" : product.getPrice(Enum_Price_MasterType.ProductShippingPrice,rate.Code).Value.ToMoneyFormated(rate.CurrencyCode);

            ws.Cells[row, 7].Value = product.MyPhotoCreationId;

            var lstProductInOption = Db.Select<OptionInProduct>(x => x.Where(y => (y.ProductId == product.Id)).OrderBy(z => (z.Id)));

            for (int i = 0; i < lstProductOption.Count; i++)
            {
                var productInOption = lstProductInOption.Where(x => (x.ProductOptionId == lstProductOption[i].Id)).FirstOrDefault();

                ws.Cells[row, 7 + i + 1].Value = productInOption != null ? lstProductOption[i].getPrice(Enum_Price_MasterType.ProductOption,rate.Code).Value.ToMoneyFormated(rate.CurrencyCode) : "";
            }
        }

        #endregion
    }
}