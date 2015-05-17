using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
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
using PhotoBookmart.DataLayer.Models.ExtraShipping;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.DataLayer.Models.Reports;
using PhotoBookmart.Helper;
using PhotoBookmart.Support;

namespace PhotoBookmart.Areas.Administration.Controllers
{
    [ABRequiresAnyRole(RoleEnum.Admin, RoleEnum.Province, RoleEnum.District, RoleEnum.Village)]
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

        [HttpGet]
        public ActionResult Add()
        {
            DoiTuong model = new DoiTuong();
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(DoiTuong model, IEnumerable<HttpPostedFileBase> FilesUp)
        {
            #region TODO: #1
            model.Id = model.Id > 0 ? model.Id : 0;
            model.HoTen = string.Format("{0}", model.HoTen).Trim();
            model.NamSinh = string.Format("{0}", model.NamSinh).Trim();
            model.ThangSinh = string.Format("{0}", model.ThangSinh).Trim();
            model.NgaySinh = string.Format("{0}", model.NgaySinh).Trim();
            model.CMTND = string.Format("{0}", model.CMTND).Trim();
            model.NoiCap = string.Format("{0}", model.NoiCap).Trim();
            model.TruQuan = string.Format("{0}", model.TruQuan).Trim();
            model.NguyenQuan = string.Format("{0}", model.NguyenQuan).Trim();
            if (!model.isKhuyetTat.HasValue || !model.isKhuyetTat.Value)
            {
                model.DangKT = null;
                model.MucDoKT = null;
            }
            model.SoQD = string.Format("{0}", model.SoQD).Trim();
            model.GhiChu = string.Format("{0}", model.GhiChu).Trim();
            DoiTuong old_model = null;
            if (model.Id > 0)
            {
                old_model = Db.Select<DoiTuong>(x => x.Where(y => y.Id == model.Id).Limit(0, 1)).FirstOrDefault();
                if (old_model != null)
                {
                    model.IDDT = old_model.IDDT;
                    model.CreatedOn = old_model.CreatedOn;
                    model.CreatedBy = old_model.CreatedBy;
                }
            }
            else
            {
                model.IDDT = Guid.NewGuid();
                model.CreatedOn = DateTime.Now;
                model.CreatedBy = CurrentUser.Id;
            }

            PermissionChecker permission = new PermissionChecker(this);
            if (!(model.Id == 0 && permission.CanAdd(model) ||
                  model.Id > 0 && permission.CanUpdate(old_model) && permission.CanUpdate(model)))
            {
                return JsonError("Vui lòng không hack ứng dụng.");
            }
            #endregion

            #region TODO: #2
            if (string.IsNullOrEmpty(model.MaHC))
            {
                return JsonError("Vui lòng chọn xã.");
            }
            if (!model.IDDiaChi.HasValue)
            {
                return JsonError("Vui lòng chọn xóm.");
            }
            if (!(Db.Count<DanhMuc_HanhChinh>(x => x.MaHC == model.MaHC) > 0 &&
                  Db.Count<DanhMuc_DiaChi>(x => x.MaHC == model.MaHC && x.IDDiaChi == model.IDDiaChi) > 0))
            {
                return JsonError("Vui lòng không hack ứng dụng.");
            }
            #endregion

            #region TODO: #3
            if (string.IsNullOrEmpty(model.HoTen))
            {
                return JsonError("Vui lòng nhập họ & tên.");
            }
            if (string.IsNullOrEmpty(model.NamSinh))
            {
                return JsonError("Vui lòng nhập ngày sinh » Năm.");
            }
            if (!new Regex(@"^([1-9]\d{3})$", RegexOptions.Compiled).IsMatch(model.NamSinh) ||
                int.Parse(model.NamSinh) < DateTime.MinValue.Year ||
                int.Parse(model.NamSinh) > DateTime.MaxValue.Year)
            {
                return JsonError("Ngày sinh » Năm không đúng định dạng.");
            }
            if (!string.IsNullOrEmpty(model.NgaySinh) && string.IsNullOrEmpty(model.ThangSinh))
            {
                return JsonError("Vui lòng nhập ngày sinh » Tháng.");
            }
            if (!string.IsNullOrEmpty(model.ThangSinh) && !new Regex(@"^(0?[1-9]|1[012])$", RegexOptions.Compiled).IsMatch(model.ThangSinh))
            {
                return JsonError("Ngày sinh » Tháng không đúng định dạng.");
            }
            if (!string.IsNullOrEmpty(model.NgaySinh))
            {
                DateTime dt = new DateTime(int.Parse(model.NamSinh), int.Parse(model.ThangSinh), 1).AddMonths(1).Subtract(TimeSpan.FromSeconds(1));
                if (!new Regex(@"^(0?[1-9]|[12][0-9]|3[01])$", RegexOptions.Compiled).IsMatch(model.NgaySinh) || int.Parse(model.NgaySinh) > dt.Day)
                {
                    return JsonError("Ngày sinh » Ngày không đúng định dạng.");
                }
            }
            model.ThangSinh = !string.IsNullOrEmpty(model.ThangSinh) && model.ThangSinh.Length < 2 ? "0" + model.ThangSinh : model.ThangSinh;
            model.NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) && model.NgaySinh.Length < 2 ? "0" + model.NgaySinh : model.NgaySinh;
            if (string.IsNullOrEmpty(model.GioiTinh))
            {
                return JsonError("Vui lòng chọn giới tính.");
            }
            if (!model.MaDanToc.HasValue)
            {
                return JsonError("Vui lòng chọn dân tộc.");
            }
            if (string.IsNullOrEmpty(model.CMTND))
            {
                return JsonError("Vui lòng nhập CMTND.");
            }
            if (!model.NgayCap.HasValue)
            {
                return JsonError("Vui lòng nhập ngày cấp.");
            }
            if (string.IsNullOrEmpty(model.NoiCap))
            {
                return JsonError("Vui lòng nhập nơi cấp.");
            }
            if (string.IsNullOrEmpty(model.TruQuan))
            {
                return JsonError("Vui lòng nhập trú quán.");
            }
            if (string.IsNullOrEmpty(model.NguyenQuan))
            {
                return JsonError("Vui lòng nhập nguyên quán.");
            }
            if (!(new string[] { "Male", "Female" }.Contains(model.GioiTinh) &&
                  Db.Count<DanhMuc_DanToc>(x => x.Id == model.MaDanToc.Value) > 0))
            {
                return JsonError("Vui lòng không hack ứng dụng.");
            }
            #endregion

            #region TODO: #4
            if (string.IsNullOrEmpty(model.MaLDT))
            {
                return JsonError("Vui lòng chọn loại.");
            }
            if (model.MaLDT.StartsWith("01"))
            {
                if (model.MaLDT_Details.Count != 1)
                {
                    return JsonError("Vui lòng không hack ứng dụng.");
                }
                DoiTuong_LoaiDoiTuong_CT detail = new DoiTuong_LoaiDoiTuong_CT();
                detail.Type1_InfoFather = string.Format("{0}", model.MaLDT_Details[0].Type1_InfoFather).Trim();
                detail.Type1_InfoMother = string.Format("{0}", model.MaLDT_Details[0].Type1_InfoMother).Trim();
                if (string.IsNullOrEmpty(detail.Type1_InfoFather))
                {
                    return JsonError("Vui lòng nhập thông tin cha.");
                }
                if (string.IsNullOrEmpty(detail.Type1_InfoMother))
                {
                    return JsonError("Vui lòng nhập thông tin mẹ.");
                }
                model.MaLDT_Details[0] = detail;
            }
            else if (model.MaLDT.StartsWith("03"))
            {
                if (model.MaLDT_Details.Count == 0)
                {
                    return JsonError("Vui lòng thêm thông tin » con.");
                }
                List<DoiTuong_LoaiDoiTuong_CT> details = new List<DoiTuong_LoaiDoiTuong_CT>();
                foreach (DoiTuong_LoaiDoiTuong_CT item in model.MaLDT_Details)
                {
                    DoiTuong_LoaiDoiTuong_CT detail = new DoiTuong_LoaiDoiTuong_CT();
                    detail.Id = item.Id;
                    detail.Type3_FullName = string.Format("{0}", item.Type3_FullName).Trim();
                    detail.Type3_DateOfBirth = item.Type3_DateOfBirth;
                    detail.Type3_DateOfBirth_IsMonth = item.Type3_DateOfBirth_IsMonth;
                    detail.Type3_DateOfBirth_IsDate = item.Type3_DateOfBirth_IsDate;
                    detail.Type3_Gender = item.Type3_Gender;
                    detail.Type3_CurrAddr = string.Format("{0}", item.Type3_CurrAddr).Trim();
                    detail.Type3_StatusLearn = string.Format("{0}", item.Type3_StatusLearn).Trim();
                    if (string.IsNullOrEmpty(detail.Type3_FullName))
                    {
                        return JsonError("Vui lòng kiểm tra lại họ & tên cho các con.");
                    }
                    if (detail.Type3_DateOfBirth.Year == DateTime.MinValue.Year)
                    {
                        return JsonError("Vui lòng kiểm tra lại ngày sinh cho các con » Năm.");
                    }
                    if (!detail.Type3_DateOfBirth_IsMonth && detail.Type3_DateOfBirth.Month != 1)
                    {
                        return JsonError("Vui lòng kiểm tra lại ngày sinh cho các con » Tháng.");
                    }
                    if (!detail.Type3_DateOfBirth_IsDate && detail.Type3_DateOfBirth.Day != 1)
                    {
                        return JsonError("Vui lòng kiểm tra lại ngày sinh cho các con » Ngày.");
                    }
                    if (string.IsNullOrEmpty(detail.Type3_Gender) || !new string[] { "Male", "Female" }.Contains(detail.Type3_Gender))
                    {
                        return JsonError("Vui lòng kiểm tra lại giới tính cho các con.");
                    }
                    details.Add(detail);
                }
                model.MaLDT_Details = details;
                if (model.Id == 0 && details.Count(x => x.Id > 0) > 0)
                {
                    return JsonError("Vui lòng không hack ứng dụng.");
                }
            }
            else if (model.MaLDT.StartsWith("04"))
            {
                if (model.MaLDT_Details.Count != 1)
                {
                    return JsonError("Vui lòng không hack ứng dụng.");
                }
                DoiTuong_LoaiDoiTuong_CT detail = new DoiTuong_LoaiDoiTuong_CT();
                detail.Type4_MaritalStatus = model.MaLDT_Details[0].Type4_MaritalStatus;
                detail.Type4_InfoAdditional = string.Format("{0}", model.MaLDT_Details[0].Type4_InfoAdditional).Trim();
                if (string.IsNullOrEmpty(detail.Type4_MaritalStatus))
                {
                    return JsonError("Vui lòng chọn tình trạng hôn nhân.");
                };
                model.MaLDT_Details[0] = detail;
            }
            else if (model.MaLDT.StartsWith("05"))
            {
                if (model.MaLDT_Details.Count != 1)
                {
                    return JsonError("Vui lòng không hack ứng dụng.");
                }
                DoiTuong_LoaiDoiTuong_CT detail = new DoiTuong_LoaiDoiTuong_CT();
                detail.Type5_SelfServing = model.MaLDT_Details[0].Type5_SelfServing;
                detail.Type5_Carer = string.Format("{0}", model.MaLDT_Details[0].Type5_Carer).Trim();
                if (string.IsNullOrEmpty(detail.Type5_SelfServing))
                {
                    return JsonError("Vui lòng chọn khả năng phục vụ.");
                }
                model.MaLDT_Details[0] = detail;
            }
            else
            {
                if (model.MaLDT_Details.Count > 0)
                {
                    return JsonError("Vui lòng không hack ứng dụng.");
                }
            }
            if (model.isKhuyetTat.HasValue && model.isKhuyetTat.Value)
            {
                if (!model.DangKT.HasValue)
                {
                    return JsonError("Vui lòng chọn dạng khuyết tật.");
                }
                if (!model.MucDoKT.HasValue)
                {
                    return JsonError("Vui lòng chọn mức độ khuyết tật.");
                }
            }
            if (model.Id > 0 && model.MaLDT_Details.Count(x => x.Id > 0) > 0 && model.MaLDT_Details.Count(x => x.Id > 0) != Db.Count<DoiTuong_LoaiDoiTuong_CT>(x => x.CodeObj == model.IDDT && Sql.In(x.Id, model.MaLDT_Details.Where(y => y.Id > 0).Select(y => y.Id))) ||
                model.isKhuyetTat.HasValue && model.isKhuyetTat.Value && (Db.Count<DanhMuc_DangKhuyetTat>(x => x.IDDangTat == model.DangKT.Value) == 0 || Db.Count<DanhMuc_MucDoKhuyetTat>(x => x.IDMucDoKT == model.MucDoKT.Value) == 0))
            {
                return JsonError("Vui lòng không hack ứng dụng.");
            }
            #endregion

            #region TODO: #5
            if (model.MucTC.HasValue && model.MucTC.Value < 0)
            {
                return JsonError("Mức trợ cấp không đúng định dạng.");
            }
            #endregion
            
            using (IDbTransaction dbTrans = Db.OpenTransaction())
            {
                Db.Delete<DoiTuong_LoaiDoiTuong_CT>(x => x.CodeObj == model.IDDT && x.CodeType != model.MaLDT);
                if (model.MaLDT.StartsWith("03"))
                {
                    List<long> ids = new List<long>() { 0 };
                    ids.AddRange(model.MaLDT_Details.Where(x => x.Id > 0).Select(x => x.Id));
                    Db.Delete<DoiTuong_LoaiDoiTuong_CT>(x => x.CodeObj == model.IDDT && !Sql.In(x.Id, ids));
                }

                if (!model.MaLDT.StartsWith("03") && model.MaLDT_Details.Count == 1)
                {
                    DoiTuong_LoaiDoiTuong_CT detail = Db.Select<DoiTuong_LoaiDoiTuong_CT>(x => x.Where(y => y.CodeObj == model.IDDT).Limit(0, 1)).FirstOrDefault();
                    if (detail != null) { model.MaLDT_Details[0].Id = detail.Id; }
                }
                model.MaLDT_Details.ForEach(x => {
                    x.CodeObj = model.IDDT;
                    x.CodeType = model.MaLDT;
                });

                Db.Save(model);
                Db.UpdateAll<DoiTuong_LoaiDoiTuong_CT>(model.MaLDT_Details.Where(x => x.Id > 0));
                Db.InsertAll<DoiTuong_LoaiDoiTuong_CT>(model.MaLDT_Details.Where(x => x.Id == 0));
                dbTrans.Commit();
            }
            return JsonSuccess(Url.Action("Index", "WebsiteProduct", new { }), null);
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

        [HttpPost]
        public ActionResult GetProvincesForFilter()
        {
            return Json(GetAllProvinces());
        }

        [HttpPost]
        public ActionResult GetDistrictsForFilter(string MaHC)
        {
            return Json(GetDistrictsByProvince(MaHC));
        }

        [HttpPost]
        public ActionResult GetVillagesForFilter(string MaHC)
        {
            return Json(GetVillagesByDistrict(MaHC));
        }

        [HttpPost]
        public ActionResult GetHamletsForFilter(string MaHC)
        {
            return Json(GetHamletsByVillage(MaHC));
        }

        [HttpPost]
        public ActionResult GetEthnicsForFilter()
        {
            return Json(Cache_GetAllEthnics());
        }

        [HttpPost]
        public ActionResult GetTypesDisabilityForFilter()
        {
            return Json(Cache_GetAllTypesDisability());
        }

        [HttpPost]
        public ActionResult GetLevelsDisabilityForFilter()
        {
            return Json(Cache_GetAllLevelsDisability());
        }

        [HttpPost]
        public ActionResult GetTypesObjForFilter()
        {
            return Json(Cache_GetAllTypesObj());
        }

        [HttpPost]
        public ActionResult GetMaritalStatusesForFilter()
        {
            return Json(Cache_GetAllMaritalStatuses());
        }

        [HttpPost]
        public ActionResult GetSelfServingsForFilter()
        {
            return Json(Cache_GetAllSelfServings());
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
