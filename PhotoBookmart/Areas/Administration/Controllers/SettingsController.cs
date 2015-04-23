using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoBookmart.Areas.Administration.Models;
using PhotoBookmart.Common.Helpers;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using PhotoBookmart.DataLayer.Models.System;

namespace PhotoBookmart.Areas.Administration.Controllers
{
    [RequiredRole("Admin")]
    public class SettingsController : WebAdminController
    {
        [HttpGet]
        public ActionResult Index(int? page)
        {
            int totalItem = (int)Db.Count<Settings>();
            int totalPage = (int)Math.Ceiling((double)totalItem / ITEMS_PER_PAGE);
            int currPage = (page != null && page.Value > 0 && page < totalPage + 1) ? page.Value : 1;

            ViewData["CurrPage"] = currPage;
            return View();
        }

        [HttpGet]
        public ActionResult Add()
        {
            Settings model = new Settings();
            return View("CreateOrUpdate", model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var model = Db.Select<Settings>(x => x.Where(y => (y.Id == id)).Limit(0, 1)).FirstOrDefault();
            if (model == null) return RedirectToAction("Index", "Settings", new { page = 1 });
            return View("CreateOrUpdate", model);
        }

        [HttpPost]
        public ActionResult CreateOrUpdate(Settings model, string isContinue)
        {
            if (model.Key != null) { model.Key = model.Key.Trim(); }
            Settings modelUpdated = model.Id > 0 ? Db.Select<Settings>(x => x.Where(y => (y.Id == model.Id)).Limit(0, 1)).FirstOrDefault() :  new Settings();
            
            if (modelUpdated == null ||
                model.Key != Enum_Settings_Key.NONE.ToString() && Settings_Key_GetAll().Where(x => (x.Id == model.Key)).Count() == 0)
            {
                ViewBag.Error = "Vui lòng không hack ứng dụng.";
                return View("CreateOrUpdate", model);
            }
            if (model.Key == Enum_Settings_Key.NONE.ToString())
            {
                ViewBag.Error = "Vui lòng chọn mã.";
                return View("CreateOrUpdate", model);
            }
            if (string.IsNullOrEmpty(model.Value))
            {
                ViewBag.Error = "Vui lòng nhập giá trị.";
                return View("CreateOrUpdate", model);
            }
            
            if (Db.Count<Settings>(x => x.Key == model.Key && x.Id != model.Id) > 0)
            {
                ViewBag.Error = "Mã đã được sử dụng, vui lòng chọn mã khác.";
                return View("CreateOrUpdate", model);
            }
            
            if (model.Id == 0)
            {
                Db.Insert<Settings>(model);
            }
            else
            {
                Db.UpdateOnly<Settings>(new Settings()
                {
                    Key = model.Key,
                    Value = model.Value,
                    Desc = model.Desc,
                },
                ev => ev.Update(p => new
                {
                    p.Key,
                    p.Value,
                    p.Desc
                }).Where(m => (m.Id == model.Id)));
            }

            bool addNew = false;
            bool.TryParse(isContinue, out addNew);
            return addNew ? RedirectToAction("Add", "Settings", new { }) : RedirectToAction("Index", "Settings", new { page = 1 });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                Db.DeleteById<Settings>(id);
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult List(int page)
        {
            int pageSize = ITEMS_PER_PAGE;
            int totalItem = (int)Db.Count<Settings>();
            int totalPage = (int)Math.Ceiling((double)totalItem / pageSize);
            List<Settings> model = Db.Select<Settings>().Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["CurrPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalItem"] = totalItem;
            ViewData["TotalPage"] = totalPage;
            return PartialView("_List", model);
        }

        public static List<ListModel> Settings_Key_GetAll()
        {
            List<ListModel> result = new List<ListModel>();
            foreach (Enum_Settings_Key setting in (Enum_Settings_Key[])Enum.GetValues(typeof(Enum_Settings_Key)) ?? Enumerable.Empty<Enum_Settings_Key>())
            {
                result.Add(new ListModel() { Id = setting.ToString(), Name = setting.ToString() != "NONE" ? setting.ToString() : "- - Chọn - -" });
            }
            return result;
        }
    }
}
