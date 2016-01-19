using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Common.Utils;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

using ABSoft.Photobookmart.FTPSync.Helper.ORM;
using ABSoft.Photobookmart.FTPSync.Helper.Provider;
using ABSoft.Photobookmart.FTPSync.ServiceInterface;
using System.Windows.Forms;
using System.Data;
using ABSoft.Photobookmart.FTPSync.Components;

namespace ABSoft.Photobookmart.FTPSync
{
    public enum Env
    {
        Local,
        Dev,
        Test,
        Prod
    }

    public partial class AppConfig
    {
        public Env Env { get; set; }

        public List<string> AdminUserNames { get; set; }

        public AppConfig(IResourceManager appSettings)
        {
            this.Env = appSettings.Get("Env", Env.Local);

            this.AdminUserNames = appSettings.Get("AdminUserNames", new List<string>());
        }
    }

    public partial class AppHost : AppHostBase
    {
        public static IDbConnection DatabaseConnection;
        public static OrmLiteConnectionFactory dbFactory;
        /// <summary>
        ///  Share FTP Service among other threads
        /// </summary>
        public static FTPSyncService FTPService { get; set; }

        public static IRBLog Log { get; set; }

        public static string ServiceName
        {
            get
            {
                return "Photobookmart FTP Sync";
            }
        }

        public static string ServiceDescription
        {
            get
            {
                return "To sync all photobookmart from remote server to local server by ftp";
            }
        }

        public AppHost() : base("Photobookmart - FTP Sync", typeof(TTGAPI).Assembly) { }

        public static AppConfig AppConfig;

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~AppHost()
        {
            Dispose();
        }

        public override void Configure(Funq.Container container)
        {
            SetConfig(new EndpointHostConfig
            {
                DebugMode = false,

                GlobalResponseHeaders =
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" }
                },

                EnableFeatures = ServiceStack.ServiceHost.Feature.All,

                ServiceStackHandlerFactoryPath = "api"
            });

            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

            var appSettings = new AppSettings();

            AppConfig = new AppConfig(appSettings);

            container.Register(AppConfig);

            container.Register<ICacheClient>(new MemoryCacheClient());

            AdzCredenticalsProvider authProvider = new AdzCredenticalsProvider();

            Plugins.Add(new SessionFeature());

            AuthFeature authFeature = new AuthFeature(() => new AuthUserSession(), new IAuthProvider[]{
                authProvider, new BasicAuthProvider()
            });

            Plugins.Add(authFeature);

            //Plugins.Add(new RegistrationFeature());

            dbFactory = GetDbConnectionFromConfig();
            container.Register<IDbConnectionFactory>(dbFactory);
            DatabaseConnection = dbFactory.OpenDbConnection();
            container.Register(c => dbFactory.OpenDbConnection()).ReusedWithin(Funq.ReuseScope.Container);

            Helper.Common.ORMCreateTableOnInit(DatabaseConnection, "admin");

            var userRep = new ABORMLiteAuthRepository(dbFactory);

            container.Register<IAuthProvider>(authProvider);

            container.Register<IUserAuthRepository>(userRep);

        }

        public static OrmLiteConnectionFactory GetDbConnectionFromConfig()
        {
            var cs = ConfigurationManager.AppSettings.Get("ConnectionString_SQLite");
            cs = cs.Replace("{PATH}", Application.StartupPath);

            SqliteOrmLiteDialectProvider dialect = SqliteOrmLiteDialectProvider.Instance;

            dialect.UseUnicode = true;

            dialect.NamingStrategy = new ABNamingStrategy();

            return new OrmLiteConnectionFactory(cs, false, dialect);
        }

        public static bool Start()
        {
            Log = new RBLog();
            Log.Log("------------------APP STARTING-----------------------");
            try
            {
                new AppHost().Init();
                return true;
            }
            catch (Exception ex)
            {
                Log.Log(ex);
                return false;
            }
        }
    }
}