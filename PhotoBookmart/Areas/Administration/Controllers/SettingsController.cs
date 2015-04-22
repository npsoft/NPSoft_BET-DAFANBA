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
    [RequiredRole("Administrator")]
    public class SettingsController : WebAdminController
    {
        public static int ITEM_PER_PAGE_RB_SETTING = 30;

        public static List<ListModel> Settings_Key_GetAll()
        {
            List<ListModel> result = new List<ListModel>();

            foreach (var e in (Enum_Settings_Key[])Enum.GetValues(typeof(Enum_Settings_Key)) ?? Enumerable.Empty<Enum_Settings_Key>())
            {
                if (e.ToString() != "NONE")
                {
                    result.Add(new ListModel() { Id = e.ToString(), Name = e.ToString() });
                }
            }

            return result;
        }

        #region [CONTROLLER] Settings

        public ActionResult Index(int? page)
        {
            int totalItem = (int)Db.Count<Settings>();

            int totalPage = (int)Math.Ceiling((double)totalItem / ITEM_PER_PAGE_RB_SETTING);

            int currPage = (page != null && page.Value > 0 && page < totalPage + 1) ? page.Value : 1;

            ViewData["CurrPage"] = currPage;

            return View();
        }

        public ActionResult List(int page)
        {
            int pageSize = ITEM_PER_PAGE_RB_SETTING;

            int totalItem = (int)Db.Count<Settings>();

            int totalPage = (int)Math.Ceiling((double)totalItem / pageSize);

            var model = Db.Select<Settings>().Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var users = Cache_GetAllUsers();

            /*foreach (var c in model ?? Enumerable.Empty<Settings>())
            {
                var user = users.Where(x => (x.Id == c.CreatedBy)).FirstOrDefault();

                c.CreatedByUsername = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.UserName) : "Deleted user";
            }*/

            ViewData["CurrPage"] = page;

            ViewData["PageSize"] = pageSize;

            ViewData["TotalItem"] = totalItem;

            ViewData["TotalPage"] = totalPage;

            return PartialView("_List", model);
        }

        public ActionResult Add()
        {
            Settings model = new Settings();

            return View("CreateOrUpdate", model);
        }

        public ActionResult Edit(int id)
        {
            var model = Db.Select<Settings>(x => x.Where(y => (y.Id == id))).FirstOrDefault();

            if (model == null) return RedirectToAction("Index", "Settings", new { page = 1 });

            return View("CreateOrUpdate", model);
        }

        [ValidateInput(false)]
        public ActionResult CreateOrUpdate(Settings model, string isContinue)
        {
            Settings modelUpdated = new Settings();

            #region Validate

            if (model.Id > 0)
            {
                modelUpdated = Db.Select<Settings>(x => x.Where(y => (y.Id == model.Id))).FirstOrDefault();

                if (modelUpdated == null)
                {
                    ViewBag.Error = "Please don't try to hack us";

                    return View("CreateOrUpdate", model);
                }
            }

            if (model.Key != null) model.Key = model.Key.Trim();

            if (string.IsNullOrEmpty(model.Key))
            {
                ViewBag.Error = "Please select field » Key";

                return View("CreateOrUpdate", model);
            }

            if (string.IsNullOrEmpty(model.Value))
            {
                ViewBag.Error = "Please enter field » Value";

                return View("CreateOrUpdate", model);
            }


            if (model.Key == Enum_Settings_Key.NONE.ToString() || Settings_Key_GetAll().Where(x => (x.Id == model.Key)).Count() == 0)
            {
                ViewBag.Error = "Please select a valid field » Key";

                return View("CreateOrUpdate", model);
            }

            if (Db.Select<Settings>(x => x.Where(y => (y.Key == model.Key && y.Id != model.Id))).FirstOrDefault() != null)
            {
                ViewBag.Error = "Key already used";

                return View("CreateOrUpdate", model);
            }

            #endregion

            if (model.Id == 0)
            {
                Db.Insert<Settings>(model);
            }
            else
            {
                Db.UpdateOnly<Settings>(new Settings()
                {
                    Key = model.Key,
                    Value = model.Value
                },
                ev => ev.Update(p => new
                {
                    p.Key,
                    p.Value
                }).Where(m => (m.Id == model.Id)));
            }

            bool addNew = false;

            bool.TryParse(isContinue, out addNew);

            if (addNew)
                return RedirectToAction("CreateOrUpdate", "Settings", new { });
            else
                return RedirectToAction("Index", "Settings", new { page = 1 });
        }

        [ValidateInput(false)]
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

        #endregion
    }
}
