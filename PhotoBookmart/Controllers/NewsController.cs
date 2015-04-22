using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RBVMC.DataLayer.Models;
using System.Data;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;
using ServiceStack.Common;
using RBVMC.Common.Helpers;
using RBVMC.DataLayer.Models.Users_Management;
using RBVMC.DataLayer.Models.Sites;
using ServiceStack.Common.Web;
using RealBonusWeb.Models;

namespace RealBonusWeb.Controllers
{
    public class NewsController : RBVMCController
    {
        int ItemPerPage = 20;

        public ActionResult Index(int? page)
        {
            var count = Db.Count<Site_News>(m => m.SiteId == CurrentWebsite.Id && m.LanguageCode == CurrentLanguage.LanguageCode && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now)));

            var pages = (int)Math.Ceiling((decimal)count / (decimal)ItemPerPage);
            var current_page = 1;
            if (page.HasValue)
                current_page = page.Value;
            if (current_page > pages && pages > 0)
            {
                current_page = pages;
            }
            var start_index = (current_page - 1) * ItemPerPage;

            // chua lam phan trang
            var model = Db.Select<Site_News>(n => n.Where(m => m.SiteId == CurrentWebsite.Id && m.LanguageCode == CurrentLanguage.LanguageCode && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now))).OrderByDescending(m => m.PublishOn).Limit(start_index, ItemPerPage));

            var list_user = Cache_GetAllUsers(CurrentWebsite.Id);
            var list_cat = Cache_GetNewsCategory(CurrentWebsite.Id);

            foreach (var item in model)
            {
                var x = list_user.Where(m => m.Id == item.CreatedBy).FirstNonDefault();
                if (x != null)
                    item.CreatedByUsername = x.DisplayName;
                else
                    item.CreatedByUsername = "Deleted User";

                var y = list_cat.Where(m => m.Id == item.CategoryId).FirstNonDefault();
                if (y == null)
                    item.Category_Name = "Deleted Category";
                else
                    item.Category_Name = y.Name;
            }

            ViewData["page"] = current_page;
            ViewData["pages"] = pages;

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult NewsSideBar()
        {
            var model = Db.Select<Site_News>(n => n.Where(m => m.SiteId == CurrentWebsite.Id && m.LanguageCode == CurrentLanguage.LanguageCode && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now))).OrderByDescending(m => m.PublishOn).Limit(4));
            if (model == null)
            {
                model = new List<Site_News>();
            }
            return View(model);
        }

        /// <summary>
        /// View News by category seo_name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ByCategory(string id, int? page)
        {
            var list_user = Cache_GetAllUsers(CurrentWebsite.Id);
            var list_cat = Cache_GetNewsCategory(CurrentWebsite.Id);
            var cat = list_cat.Where(m => m.Status && m.SeoName == id).FirstOrDefault();

            var count = Db.Count<Site_News>(m => m.SiteId == CurrentWebsite.Id && m.CategoryId == cat.Id && m.LanguageCode == CurrentLanguage.LanguageCode && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now)));

            var pages = (int)Math.Ceiling((decimal)count / (decimal)ItemPerPage);
            var current_page = 1;
            if (page.HasValue)
                current_page = page.Value;
            if (current_page > pages && pages > 0)
            {
                current_page = pages;
            }
            var start_index = (current_page - 1) * ItemPerPage;
            if (cat == null)
            {
                return RedirectToAction("Index");
            }

            var model = Db.Select<Site_News>(n => n.Where(m => m.SiteId == CurrentWebsite.Id && m.CategoryId == cat.Id && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now))).OrderByDescending(m => m.PublishOn).Limit(start_index,ItemPerPage));

            foreach (var item in model)
            {
                var x = list_user.Where(m => m.Id == item.CreatedBy).FirstNonDefault();
                if (x != null)
                    item.CreatedByUsername = x.DisplayName;
                else
                    item.CreatedByUsername = "Deleted User";

                var y = list_cat.Where(m => m.Id == item.CategoryId).FirstNonDefault();
                if (y == null)
                {
                    item.Category_Name = "Deleted Category";
                    item.Category_SeoName = "";
                }
                else
                {
                    item.Category_Name = y.Name;
                    item.Category_SeoName = y.SeoName;
                }
            }

            ViewData["page"] = current_page;
            ViewData["pages"] = pages;
            ViewData["view_by_category"] = 1;
            ViewData["cat_name"] = cat.Name;
            return View("Index", model);
        }

        /// <summary>
        /// Show news detail page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(string id)
        {
            var model = Db.Select<Site_News>(n => n.Where(m => m.SiteId == CurrentWebsite.Id && m.SeoName == id && (!m.PublishSchedule || (m.PublishSchedule && m.PublishOn <= DateTime.Now && m.UnPublishOn >= DateTime.Now))).OrderByDescending(m => m.CreatedOn).Limit(1)).FirstOrDefault();

            if (model == null)
            {
                return RedirectToAction("Index");
            }

            // increase view count
            model.Statistic_Views++;
            Db.Update<Site_News>(model);

            var list_user = Cache_GetAllUsers(CurrentWebsite.Id);
            var list_cat = Cache_GetNewsCategory(CurrentWebsite.Id);
            var x = list_user.Where(m => m.Id == model.CreatedBy).FirstNonDefault();
            if (x != null)
                model.CreatedByUsername = x.DisplayName;
            else
                model.CreatedByUsername = "Deleted User";

            var y = list_cat.Where(m => m.Id == model.CategoryId).FirstNonDefault();
            if (y == null)
                model.Category_Name = "Deleted Category";
            else
                model.Category_Name = y.Name;

            return View(model);
        }

        /// <summary>
        /// Show list of categories on left side bar
        /// </summary>
        /// <returns></returns>
        public ActionResult _Section_News_Categories()
        {
            var cats = Db.Where<Site_News_Category>(m => m.Status && m.SiteId == CurrentWebsite.Id && m.LanguageCode == CurrentLanguage.LanguageCode).OrderByDescending(m => m.CreatedOn).ToList();
            foreach (var x in cats)
            {
                x.CountNewsInside = x.New_CountInside();
            }
            return View("_Section_News_Categories",cats);
        }

    }


}