using ServiceStack.Mvc;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.OrmLite;
using System.Data;
using ServiceStack.ServiceInterface;
using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using ServiceStack.ServiceHost;
using ServiceStack.Common;
using ServiceStack;
using PhotoBookmart.Common.Json;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using PhotoBookmart.Areas.Administration.Models;
using PhotoBookmart.Support;
using PhotoBookmart.Common;
using System.Web;
using System.Web.Routing;
using PhotoBookmart.Common.Helpers;
using PhotoBookmart.DataLayer.Models.ExtraShipping;
using PhotoBookmart.DataLayer.Models.Users_Management;
using PhotoBookmart.DataLayer.Models.System;
using PhotoBookmart.DataLayer.Models.Sites;
using PhotoBookmart.DataLayer.Models.Products;
using PhotoBookmart.ServiceInterface;
using PhotoBookmart.ServiceInterface.RequestModel;
using PhotoBookmart.DataLayer.Models.SMS;
using libphonenumber;

namespace PhotoBookmart.Controllers
{
    public class BaseController : ServiceStackController<AuthUserSession>, IDisposable
    {
        //public IDbConnectionFactory DbFactory { get; set; }
        //private IDbConnection db=null;
        public IUserAuthRepository UserAuthRepo { get; set; }

        public AuthService AuthService
        {
            get
            {
                var authService = ServiceStack.WebHost.Endpoints.AppHostBase.Instance.Container.Resolve<AuthService>();
                authService.RequestContext = System.Web.HttpContext.Current.ToRequestContext();
                return authService;
            }
        }

        private LocalAPI _InternalServiceAPI = null;
        public LocalAPI InternalService
        {
            get
            {
                if (_InternalServiceAPI == null)
                {
                    _InternalServiceAPI = AppHost.ResolveService<LocalAPI>(System.Web.HttpContext.Current);
                }
                return _InternalServiceAPI;
            }
        }


        /// <summary>
        /// Rerturn Authenticated User ID for logged user
        /// </summary>
        public int AuthenticatedUserID
        {
            get
            {
                if (AuthSession.Id == null)
                    return 0;
                else
                    return int.Parse(AuthSession.Id);
            }
        }

        /// <summary>
        /// Return current session user
        /// </summary>
        public ABUserAuth CurrentUser
        {
            //var session_name = AuthService.GetSessionId() + "_UserDetail";
            //var user = Cache.Get<ABUserAuth>(session_name);
            //if (user == null)
            //{
            //    var id = AuthenticatedUserID;
            //    if (id == 0)
            //        return null;
            //    user = Db.Where<ABUserAuth>(m => m.Id == id).FirstOrDefault();
            //    // we set the cache just 2 seconds because sometime they need to get update
            //    // we just want to use this cache within one life cycle
            //    Cache.Set<ABUserAuth>(session_name, user, TimeSpan.FromSeconds(3));
            //}
            get
            {
                //var user = UserAuthRepo.GetUserAuth(AuthSession, null);
                var user = Db.Where<ABUserAuth>(m => m.Id == AuthenticatedUserID).FirstOrDefault();
                return user;
            }

        }

        IDbConnection _connection = null;
        public virtual IDbConnection Db
        {
            get
            {
                if (_connection == null)
                {
                    _connection = AppHost.Resolve<IDbConnection>();

                }

                if (_connection != null && _connection.State != ConnectionState.Open)
                {
                    // force open new connection
                    _connection = AppHost.Resolve<IDbConnectionFactory>().Open();
                }

                return _connection;
            }
        }

        // TODO: read curent languge from Session
        public Language CurrentLanguage
        {
            get
            {
                var ret = Cache.Get<Language>(AuthService.GetSessionId() + "FrontEndCurrentLanguage");
                if (ret == null)
                {
                    ret = InternalService.CurrentLanguage;
                    if (ret != null)
                        Cache.Set<Language>(AuthService.GetSessionId() + "FrontEndCurrentLanguage", ret, TimeSpan.FromMinutes(3));
                }
                return ret;
                //if (Session["FrontEndCurrentLanguage"] == null)
                //    Session["FrontEndCurrentLanguage"] = TTGService.CurrentLanguage;
                //return (Language)Session["FrontEndCurrentLanguage"];
            }
            private set
            {
                //this._CurrentLanguage = value;
            }
        }

        /// <summary>
        /// Return current site (for Front End)
        /// </summary>
        public virtual Website CurrentWebsite
        {
            get
            {
                var ret = Cache.Get<Website>(InternalService.SessionNamePrefix + "FrontEndCurrentSite");
                if (ret == null)
                {
                    ret = InternalService.FrontEnd_CurrentSite();
                    if (ret != null)
                        Cache.Set<Website>(InternalService.SessionNamePrefix + "FrontEndCurrentSite", ret, TimeSpan.FromMinutes(3));
                }
                return ret;
                //if (Session["FrontEndCurrentSite"] == null)
                //    Session["FrontEndCurrentSite"] = TTGService.FrontEnd_CurrentSite();
                //return (Website)Session["FrontEndCurrentSite"];
            }
        }

        public override string LoginRedirectUrl
        {
            get { return "/Logon?redirectTo={0}"; }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            try
            {
                // we put the Principal for authenticated user
                // requestContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
                //requestContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(7199));

                if (AuthSession.UserAuthId != null)
                {
                    requestContext.HttpContext.User = new ApplicationPrincipal(AuthSession);
                    //requestContext.HttpContext.User = new ApplicationPrincipal(this.AuthSession.UserName);
                }
                else
                {
                    requestContext.HttpContext.User = new ApplicationPrincipal();
                }
            }
            catch
            {
            }
            base.Initialize(requestContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (_connection != null)
            {
                _connection.Close();
                //_connection.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~BaseController()
        {
            Dispose(true);
        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            try
            {
                Db.Close();
            }
            catch
            {
            }
        }

        public JsonResult JsonError(string message)
        {
            JsonResponse response = new JsonResponse() { Status = JsonResponse.OperationFailure, Message = message };
            return new JsonResult() { Data = response };
        }

        public JsonResult JsonSuccess(string redirectUrl, string message = null)
        {
            JsonResponse response = new JsonResponse();
            response.Status = JsonResponse.OperationSuccess;
            response.RedirectUrl = redirectUrl;
            response.Message = message;

            return new JsonResult() { Data = response };
        }

        public bool IsValidEmailAddress(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            //Regex re = new Regex(@"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");
            //if (re.IsMatch(s))
            //    return true;
            //else
            //    return false;
            try
            {
                new MailAddress(s);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool IsNumber(string text)
        {
            Regex regex = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");
            return regex.IsMatch(text);
        }

        /// <summary>
        /// Return true if the input value is phone number
        /// </summary>
        /// <param name="phonenumber"></param>
        /// <param name="country_code"></param>
        /// <param name="require_mobile"></param>
        /// <returns></returns>
        protected bool IsValidPhoneByCountry(string phonenumber, string country_code, bool require_mobile)
        {
            // validate by google libphonenumber
            PhoneNumber num;
            try
            {
                num = PhoneNumberUtil.Instance.Parse(phonenumber, country_code);
                if (num.IsValidNumber)
                {
                    if (require_mobile && num.NumberType != PhoneNumberUtil.PhoneNumberType.MOBILE)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        // Return Id = Hash ; Name = Salt
        public ListModel PasswordGenerate(string password)
        {
            var PasswordHasher = new SaltedHash();
            string salt;
            string hash;
            PasswordHasher.GetHashAndSaltString(password, out hash, out salt);
            return new ListModel() { Id = hash, Name = salt };
        }

        [NonAction]
        // TODO: folder should be dynamic for each client
        // Can be this way: ~/Content/File/ImagesUpload/Company_name/username/image
        public string UploadFile(Int64 Id, string Name1, string Name2, IEnumerable<HttpPostedFileBase> FileUp, bool allow_all_extension = false)
        {
            var db_path = "";
            if (FileUp.Count() > 0)
            {
                if (string.IsNullOrEmpty(Name1))
                {
                    Name1 = "Default";
                }

                string folder = Name1;
                string filenamefinal = string.Empty;
                folder = EscapeName.Renamefile(Id.ToString() + "_" + folder + "_" + Name2);
                foreach (var file in FileUp)
                {
                    if (file != null)
                    {
                        if (file.ContentLength > 0)
                        {
                            var dir = Server.MapPath("~/Content/File/MediaUpload/" + folder);
                            var path = "";

                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            var ext = Path.GetExtension(file.FileName).ToLower();

                            if (allow_all_extension || ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".jpeg" || ext == ".bmp" || ext == ".pdf")
                            {

                                var filename = Id.ToString() + " " + Path.GetFileNameWithoutExtension(file.FileName);
                                filename = EscapeName.Renamefile(filename);
                                string tmp = Path.GetRandomFileName().Substring(0, 3);
                                filenamefinal = filename + "-" + tmp + ext;

                                path = Path.Combine(dir, filenamefinal);
                                db_path = "Content/File/MediaUpload/" + folder + "/" + filenamefinal;
                                file.SaveAs(path);
                            }
                        }

                    }
                }
            }
            return db_path;
        }

        #region Pre-define-data

        public static int ITEMS_PER_PAGE = 30;
        
        public List<ListModel> GetLowerRoles(RoleEnum role)
        {
            switch (role)
            {
                case RoleEnum.Admin:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.Province.ToString(), Name = RoleEnum.Province.DisplayName() },
                        new ListModel() { Id = RoleEnum.District.ToString(), Name = RoleEnum.District.DisplayName() },
                        new ListModel() { Id = RoleEnum.Village.ToString(), Name = RoleEnum.Village.DisplayName() } };
                case RoleEnum.Province:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.District.ToString(), Name = RoleEnum.District.DisplayName() },
                        new ListModel() { Id = RoleEnum.Village.ToString(), Name = RoleEnum.Village.DisplayName() } };
                case RoleEnum.District:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.Village.ToString(), Name = RoleEnum.Village.DisplayName() } };
                default:
                    return new List<ListModel>() { };
            }
        }
        
        public List<ListModel> GetHigherRoles(RoleEnum role)
        {
            switch (role)
            {
                case RoleEnum.Village:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.Admin.ToString(), Name = RoleEnum.Admin.DisplayName() },
                        new ListModel() { Id = RoleEnum.Province.ToString(), Name = RoleEnum.Province.DisplayName() },
                        new ListModel() { Id = RoleEnum.District.ToString(), Name = RoleEnum.District.DisplayName() } };
                case RoleEnum.District:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.Admin.ToString(), Name = RoleEnum.Admin.DisplayName() },
                        new ListModel() { Id = RoleEnum.Province.ToString(), Name = RoleEnum.Province.DisplayName() } };
                case RoleEnum.Province:
                    return new List<ListModel>() {
                        new ListModel() { Id = RoleEnum.Admin.ToString(), Name = RoleEnum.Admin.DisplayName() } };
                default:
                    return new List<ListModel>() { };
            }
        }
        
        public int GetLenMaHCByRole(RoleEnum role)
        {
            switch (role)
            {
                case RoleEnum.Province:
                    return 2;
                case RoleEnum.District:
                    return 5;
                case RoleEnum.Village:
                    return 10;
                default:
                    return 0;
            }
        }

        public static SelectList Gender_GetAll()
        {
            var x = new List<ListModel>() {
                new ListModel() { Id = "Male", Name = "Nam" },
                new ListModel() { Id = "Female", Name = "Nữ" } };
            return new SelectList(x, "Id", "Name");
        }

        public ABUserAuth User_GetByID(long id)
        {
            var user = Db.Where<ABUserAuth>(m => m.Id == id);
            if (user.Count > 0)
            {
                return user.First();
            }
            else
            {
                return null;
            }
        }

        public UserModel User_GetByID_ToModel(int id)
        {
            var user = Db.Where<ABUserAuth>(m => m.Id == id);
            if (user.Count > 0)
            {
                UserModel ret = new UserModel();
                UserModel.ToModel(user.First(), ref ret);
                return ret;
            }
            else
            {
                return null;
            }
        }

        public ABUserAuth User_GetByEmail(string email)
        {
            var user = Db.Where<ABUserAuth>(m => m.Email == email);
            if (user.Count > 0)
            {
                return user.First();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the default currency from settings
        /// </summary>
        /// <returns></returns>
        public string Setting_Defaultcurrency()
        {
            return (string)Settings.Get(Enum_Settings_Key.WEBSITE_CURRENCY, "", Enum_Settings_DataType.String);
        }

        /// <summary>
        /// return current country. if already login then will return user profile country, if not, then return default country
        /// </summary>
        /// <returns></returns>
        public string Setting_GetCurrentCountry()
        {
            var country = "";
            if (User.Identity.IsAuthenticated)
            {
                country = CurrentUser.Country;
            }
            else
            {
                // try to get the country from the domain
                var host = Request.Url.Authority;

                var countries = Db.Where<Country>(x => x.Status);
                var tcountry = countries.Where(x => x.Domains.Contains(host)).FirstOrDefault();
                if (tcountry != null)
                {
                    return tcountry.Code;
                }
                else
                {
                    //we find it by the default currency
                    var currency = Setting_Defaultcurrency();
                    var c = countries.Where(m => m.CurrencyCode == currency).FirstOrDefault();
                    if (c != null)
                    {
                        country = c.Code;
                    }
                }
            }
            return country;
        }

        /// <summary>
        /// Get current exchange rate regarding to this current user
        /// </summary>
        /// <returns></returns>
        public Country Setting_GetExchangeRate()
        {
            var u = CurrentUser;
            if (u != null && u.Id > 0 && !string.IsNullOrEmpty(u.Country))
            {
                // get the exchange rate by the current user country
                var c = Db.Select<Country>(x => x.Where(m => m.Code == u.Country).Limit(1)).FirstOrDefault();
                if (c != null)
                {
                    return c;
                }
            }

            // now because the above code can not get the country, 
            // get exchange rate by default currency
            var c2 = Db.Select<Country>(x => x.Where(m => m.CurrencyCode == Setting_Defaultcurrency()).Limit(1)).FirstOrDefault();
            if (c2 != null)
            {
                return c2;
            }
            else
            {
                return new Country() { ExchangeRate = 1, CurrencyCode = "", Code = "" }; // if can not, then return 1 as rate, nothing change
            }
        }

        public ABUserAuth User_GetByUsername(string username)
        {
            var user = Db.Where<ABUserAuth>(m => m.UserName == username);
            if (user.Count > 0)
            {
                return user.First();
            }
            else
            {
                return null;
            }
        }

        public bool CurrentUser_HasRole(RoleEnum role, ABUserAuth current_user = null)
        {
            return CurrentUser_HasRole(role.ToString(), current_user);
        }

        public bool CurrentUser_HasRole(RoleEnum role, RoleEnum role2, ABUserAuth current_user = null)
        {
            if (current_user == null)
                current_user = CurrentUser;
            return current_user.HasRole(role) || current_user.HasRole(role2);
        }

        public bool CurrentUser_HasRole(RoleEnum role, RoleEnum role2, RoleEnum role3, ABUserAuth current_user = null)
        {
            if (current_user == null)
                current_user = CurrentUser;
            return current_user.HasRole(role) || current_user.HasRole(role2) || current_user.HasRole(role3);
        }

        /// <summary>
        /// Return True if current User has input role
        /// </summary>
        public bool CurrentUser_HasRole(string role, ABUserAuth current_user = null)
        {
            if (current_user == null)
                current_user = User_GetByID(AuthenticatedUserID);
            return current_user.HasRole(role);
        }

        /// <summary>
        /// Return True if current User has input permission
        /// </summary>
        public bool CurrentUser_HasPermission(string permission)
        {
            var current_user = User_GetByID(AuthenticatedUserID);

            permission = permission.ToLower();

            return current_user.Permissions.Where(m => m.ToLower() == permission).Count() > 0;
        }

        public List<Country_State_ExtraShipping> GetExtraShippingByCountryCode(string countryCode = "MY")
        {
            var country = Db.Select<Country>(x => x.Where(y => (y.Status && y.Code == countryCode)).Limit(1)).FirstOrDefault();

            if (country != null)
            {
                return Db.Select<Country_State_ExtraShipping>(x => x.Where(y => y.CountryId == country.Id));
            }

            return null;
        }

        #endregion

        #region For Theme

        /// <summary>
        /// Render Partial View to string by using Original Razor Engine
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        public Theme Theme_GetCurrent()
        {
            var current_theme = Session.Get<Theme>("Current_Theme");
            if (current_theme == null)
            {
                // current theme is null, need to get default and set
                // TODO: Currently just get the first  theme in list
                current_theme = Db.Where<Theme>(m => m.Status).Take(1).First();
                Theme_SetDefault(current_theme);
            }
            return current_theme;
        }

        public void Theme_SetDefault(Theme theme)
        {
            Session.Set<Theme>("Current_Theme", theme);
        }
        #endregion

        #region Cache Common
        /// <summary>
        /// Get the cache of all languages 
        /// Common usage
        /// </summary>
        public List<Language> Cache_GetAllLanguage(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Language>>(InternalService.SessionNamePrefix + "TTG_Cache_Languages");
            if (data == null)
            {
                data = Db.Select<Language>().ToList();
                Cache.Add<List<Language>>(InternalService.SessionNamePrefix + "TTG_Cache_Languages", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        /// <summary>
        /// Get the cache of all Website
        /// Common usage
        /// </summary>
        public Website Cache_GetWebSite(int cache_in_minutes = 10)
        {
            var data = Cache.Get<Website>(InternalService.SessionNamePrefix + "TTG_Cache_GetWebsite");
            if (data == null)
            {
                data = Db.Select<Website>().FirstOrDefault();
                if (data != null)
                {
                    Cache.Add<Website>(InternalService.SessionNamePrefix + "TTG_Cache_GetWebsite", data, TimeSpan.FromMinutes(cache_in_minutes));
                }
            }
            return data;
        }

        /// <summary>
        /// Get all membership group. 
        /// </summary>
        /// <param name="cache_in_minutes"></param>
        /// <returns></returns>
        public List<Site_MemberGroup> Cache_GetMembershipGroup(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_MemberGroup>>(InternalService.SessionNamePrefix + "TTG_Cache_MembershipGroup");
            if (data == null)
            {

                data = Db.Select<Site_MemberGroup>();

                Cache.Add<List<Site_MemberGroup>>(InternalService.SessionNamePrefix + "TTG_Cache_MembershipGroup", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }


        public List<Site_News_Category> Cache_GetNewsCategory(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_News_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_NewsCategory");
            if (data == null)
            {
                data = Db.Select<Site_News_Category>().ToList();
                Cache.Add<List<Site_News_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_NewsCategory", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        public List<Site_News> Cache_GetNewsCategoryBySite(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_News>>(InternalService.SessionNamePrefix + "TTG_Cache_News");
            if (data == null)
            {

                data = Db.Select<Site_News>().ToList();

                Cache.Add<List<Site_News>>(InternalService.SessionNamePrefix + "TTG_Cache_News", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        public List<Site_News> Cache_GetNewsCategoryById(int cat_id = 0, int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_News>>(InternalService.SessionNamePrefix + "TTG_Cache_News_Cat_id" + cat_id);
            if (data == null)
            {
                if (cat_id == 0)
                {
                    data = Db.Select<Site_News>().ToList();
                }
                else
                {
                    data = Db.Where<Site_News>(m => m.CategoryId == cat_id);
                }
                Cache.Add<List<Site_News>>(InternalService.SessionNamePrefix + "TTG_Cache_News_Cat_id" + cat_id, data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }


        public List<Site_Blog_Category> Cache_GetBlogCategory(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_Blog_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_BlogCategory");

            if (data == null)
            {
                data = Db.Select<Site_Blog_Category>().ToList();
                Cache.Add<List<Site_Blog_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_BlogCategory", data, TimeSpan.FromMinutes(cache_in_minutes));
            }

            return data;
        }

        public List<Site_Blog> Cache_GetBlogCategoryBySite(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Site_Blog>>(InternalService.SessionNamePrefix + "TTG_Cache_Blog");

            if (data == null)
            {
                data = Db.Select<Site_Blog>().ToList();
                Cache.Add<List<Site_Blog>>(InternalService.SessionNamePrefix + "TTG_Cache_Blog", data, TimeSpan.FromMinutes(cache_in_minutes));
            }

            return data;
        }

        public List<Product_Category> Cache_GetProductCategory(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Product_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_ProductCat");
            if (data == null)
            {
                data = Db.Select<Product_Category>().ToList();
                Cache.Add<List<Product_Category>>(InternalService.SessionNamePrefix + "TTG_Cache_ProductCat", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        public List<Product> Cache_GetProduct(int cache_in_minutes = 10)
        {
            var data = Cache.Get<List<Product>>(InternalService.SessionNamePrefix + "TTG_Cache_Product");
            if (data == null)
            {
                data = Db.Select<Product>().ToList();
                Cache.Add<List<Product>>(InternalService.SessionNamePrefix + "TTG_Cache_Product", data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        /// <summary>
        /// Get cache of all users
        /// Common Usage
        /// </summary>
        public List<ABUserAuth> Cache_GetAllUsers(int cache_in_minutes = 10)
        {
            var key = InternalService.SessionNamePrefix + "TTG_Cache_Users";
            var data = Cache.Get<List<ABUserAuth>>(key);
            if (data == null)
            {
                data = Db.Select<ABUserAuth>().ToList();
                Cache.Add<List<ABUserAuth>>(key, data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        /// <summary>
        /// Return all countries by cache
        /// </summary>
        /// <param name="cache_in_minutes"></param>
        /// <returns></returns>
        public List<Country> Cache_GetAllCountry(int cache_in_minutes = 10)
        {
            var key = InternalService.SessionNamePrefix + "Cache_GetAllCountry";
            var data = Cache.Get<List<Country>>(key);
            if (data == null)
            {
                data = Db.Select<Country>().ToList();
                Cache.Add<List<Country>>(key, data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }

        public List<SiteNewsletter> Cache_GetAllNewsletter(int cache_in_minutes = 10)
        {
            var key = InternalService.SessionNamePrefix + "TTG_Cache_Newsletter";
            var data = Cache.Get<List<SiteNewsletter>>(key);
            if (data == null)
            {
                data = Db.Select<SiteNewsletter>();
                Cache.Add<List<SiteNewsletter>>(key, data, TimeSpan.FromMinutes(cache_in_minutes));
            }
            return data;
        }
        #endregion

        #region For Mailling List
        /// <summary>
        /// Return best matched mailling list template 
        /// </summary>
        /// <param name="systemname"></param>
        /// <returns></returns>
        [NonAction]
        public Site_MaillingListTemplate Get_MaillingListTemplate(string systemname)
        {
            //return Db.Where<Site_MaillingListTemplate>(m => m.Status && m.Systemname == systemname).FirstOrDefault();
            return Db.Select<Site_MaillingListTemplate>(x => x.Where(m => m.Status && m.Systemname == systemname).Limit(1)).FirstOrDefault();
        }

        #endregion

        #region For SMS
        /// <summary>
        /// Return the sms template by country
        /// </summary>
        /// <param name="systemname"></param>
        /// <returns></returns>
        [NonAction]
        protected SMSTemplateModel SMS_GetTemplate(string systemname, string country_code = "")
        {
            if (string.IsNullOrEmpty(country_code))
            {
                country_code = Setting_GetCurrentCountry();
            }

            var t = Db.Select<SMSTemplateModel>(x => x.Where(m => m.SystemName == systemname && m.CountryCode == country_code).Limit(1)).FirstOrDefault();
            return t;
        }

        /// <summary>
        /// Nomalize the phone number by convert it to the international format
        /// </summary>
        /// <param name="phonenumber"></param>
        /// <param name="country_prefix"></param>
        /// <returns></returns>
        protected string SMS_Normalize_PhoneNumber(string phonenumber, string country_prefix, string countrycode)
        {
            if (phonenumber.StartsWith("0") && phonenumber.Length > 1)
            {
                phonenumber = country_prefix + phonenumber.Substring(1);
            }
            else
            {
                // not start by zero, then maybe we need
                phonenumber = "+" + phonenumber;
            }

            // validate by google libphonenumber
            PhoneNumber num;
            try
            {
                num = PhoneNumberUtil.Instance.Parse(phonenumber, countrycode);
                if (!(num.IsValidNumber && num.NumberType == PhoneNumberUtil.PhoneNumberType.MOBILE))
                {
                    phonenumber = "";
                }
                else
                {
                    phonenumber = num.Format(PhoneNumberUtil.PhoneNumberFormat.E164);
                }
            }
            catch
            {
                return ""; // can not validate
            }

            return phonenumber;
        }
        #endregion

        #region Send Mail
        /// <summary>
        /// Render input string by token replacement before send email
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        protected void SendMail_RenderBeforeSend(ref string body, ref string title, Dictionary<string, string> input = null)
        {
            // prepare the Dictionary
            if (input == null)
            {
                input = new Dictionary<string, string>();
            }
            var parameters = new Dictionary<string, string>();
            parameters.Add(EnumMaillingListTokens.current_date_time.ToString(), DateTime.Now.ToLongDateString());
            parameters.Add(EnumMaillingListTokens.user_ipaddress.ToString(), InternalService.CurrentUserIP);
            if (CurrentUser != null)
            {
                parameters.Add(EnumMaillingListTokens.user_name.ToString(), CurrentUser.DisplayName);
                parameters.Add(EnumMaillingListTokens.user_username.ToString(), CurrentUser.UserName);
                parameters.Add(EnumMaillingListTokens.user_email.ToString(), CurrentUser.Email);
            }
            else
            {
                parameters.Add(EnumMaillingListTokens.user_name.ToString(), "");
                parameters.Add(EnumMaillingListTokens.user_username.ToString(), "");
                parameters.Add(EnumMaillingListTokens.user_email.ToString(), "");
            }

            var domain_name = InternalService.CurrentWebsiteDomainURL;
            parameters.Add(EnumMaillingListTokens.website_domain.ToString(), domain_name);
            if (CurrentWebsite != null)
            {
                parameters.Add(EnumMaillingListTokens.website_name.ToString(), CurrentWebsite.Name);
                parameters.Add(EnumMaillingListTokens.website_admin_email.ToString(), CurrentWebsite.Email_Admin);
                parameters.Add(EnumMaillingListTokens.website_info_email.ToString(), CurrentWebsite.Email_Support);
                parameters.Add(EnumMaillingListTokens.website_contact_email.ToString(), CurrentWebsite.Email_Contact);

            }
            else
            {
                parameters.Add(EnumMaillingListTokens.website_name.ToString(), CurrentWebsite.Name);
                parameters.Add(EnumMaillingListTokens.website_admin_email.ToString(), CurrentWebsite.Email_Admin);
                parameters.Add(EnumMaillingListTokens.website_info_email.ToString(), CurrentWebsite.Email_Support);
                parameters.Add(EnumMaillingListTokens.website_contact_email.ToString(), CurrentWebsite.Email_Contact);
            }
            // merge 2 dictionary into one
            Dictionary<string, string> final_input = parameters.MergeLeft(input);
            //
            body = SendMail_RenderBeforeSend_Regex(body, final_input);
            title = SendMail_RenderBeforeSend_Regex(title, final_input);
            // for domain host url replace
            body = body.Replace("href=\"/", "href=\"" + domain_name);
            body = body.Replace("src=\"/", "src=\"" + domain_name);
            body = body.Replace("href='/", "href='" + domain_name);
            body = body.Replace("src='/", "src='" + domain_name);
        }

        /// <summary>
        /// Based on Model, render template for send mail
        /// </summary>
        /// <param name="st"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        string SendMail_RenderBeforeSend_Regex(string st, Dictionary<string, string> model)
        {
            Regex re = new Regex(@"#[a-z_A-Z0-9]+");
            MatchCollection mc = re.Matches(st);
            int mIdx = 0;
            foreach (Match m in mc)
            {
                for (int gIdx = 0; gIdx < m.Groups.Count; gIdx++)
                {
                    var key = m.Groups[gIdx].Value.Substring(1); // remove first #\

                    // now search for the key in model
                    if (model.ContainsKey(key))
                    {
                        var value = model[key];
                        st = st.Replace("#" + key, value);
                    }
                    //EnumMaillingListTokens enum_key = EnumMaillingListTokens.not_match_any_key;

                    //if (Enum.TryParse<EnumMaillingListTokens>(key, out enum_key))
                    //{
                    //    // key found
                    //    var value = model[enum_key];
                    //    st = st.Replace("#" + key, value);
                    //}
                }
                mIdx++;
            }
            return st;
        }

        protected void SendMail(EmailHelper tTemplate)
        {
            var title = tTemplate.Title;
            var body = tTemplate.Body;
            SendMail_RenderBeforeSend(ref body, ref title, tTemplate.Parameters);
            tTemplate.Title = title;
            tTemplate.Body = body;
            PhotoBookmart.Common.Helpers.SendEmail.SendMail(tTemplate);
        }

        protected void SendMail(string title, string body, string receiver, string sender, string sender_name, Dictionary<string, string> input)
        {
            SendMail_RenderBeforeSend(ref body, ref title, input);
            // replace before 
            PhotoBookmart.Common.Helpers.SendEmail.SendMail(receiver, title, body, sender, sender_name);
        }
        #endregion

        #region For Language Translation
        /// <summary>
        /// Return translation for inputted key
        /// By default, this function will auto detect current language and site to get the most benefit translation
        /// You can define lang_id  by your own
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string T(string key, long lang_id = -1)
        {
            if (lang_id == -1 && CurrentLanguage != null)
            {
                lang_id = CurrentLanguage.Id;
            }

            return InternalService.Language_GetTranslation(key, lang_id);
        }

        public virtual string T(string key, string lang_code)
        {
            return InternalService.Language_GetTranslation(key, lang_code);
        }
        #endregion
    }

    #region Custom Views Type



    public abstract class RBVMCViews<TModel> : RBVMCViewsBase<TModel>
    {

    }

    public abstract class AdminRBVMCViews<TModel> : RBVMCViewsBase<TModel>
    {

        public override Website CurrentWebsite
        {
            get
            {
                var session_name = InternalService.SessionNamePrefix + "_CURRENT_WEBSITE";
                var ret = Cache.Get<Website>(session_name);
                if (ret == null)
                {
                    ret = Db.Where<Website>(m => m.Status == 1).FirstOrDefault();
                    InternalService.Post(ret.TranslateTo<WebSite_SetCurrent>());
                }

                // if still null
                if (ret == null)
                    ret = new Website();
                return ret;
            }
        }
    }
    #endregion
}