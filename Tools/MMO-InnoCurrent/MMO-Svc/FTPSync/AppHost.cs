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
using ServiceStack.OrmLite.SqlServer; /* using ServiceStack.OrmLite.Sqlite; | I:\NPSoft_HTQLNHCS\Tools\MMO-InnoCurrent\MMO-Svc\packages\ServiceStack.OrmLite.Sqlite32.3.9.71\lib\net40\ServiceStack.OrmLite.SqliteNET.dll*/
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
            var cs = ConfigurationManager.AppSettings.Get("Connection_String");
            SqlServerOrmLiteDialectProvider dialect = SqlServerOrmLiteDialectProvider.Instance;
            dialect.UseUnicode = true;
            dialect.UseDatetime2(true);
            dialect.StringColumnDefinition = "nvarchar(MAX)";
            dialect.StringLengthColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.StringLengthNonUnicodeColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.StringLengthUnicodeColumnDefinitionFormat = dialect.StringColumnDefinition;
            dialect.NamingStrategy = new ABNamingStrategy();
            var ret = new OrmLiteConnectionFactory(cs, dialect);
            ret.AutoDisposeConnection = true;
            return ret;
        }

        public static bool Start()
        {
            Log = new RBLog();
            Log.Log("------------------ APPLICATION STARTING");
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
