using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;

using ABSoft.Photobookmart.FTPSync.Models;

namespace ABSoft.Photobookmart.FTPSync.Helper
{
    public partial class Common
    {
        public static void ORMCreateTableOnInit(IDbConnection dbConn, string user, bool gainPermission = false)
        {
            dbConn.CreateTableIfNotExists<ABUserAuth>();

            dbConn.CreateTableIfNotExists<FTPConfig>();

            dbConn.CreateTableIfNotExists<FTPFileCheck>();

            #region default config
            if (dbConn.Count<FTPConfig>() == 0)
            {
                FTPConfig c = new FTPConfig() { FTPDefaultPath = "/digilabs", FTPHost = "127.0.0.1", LocalPath = @"E:\ftpsync", LocalTimeZone = 7, ServerTimeZone = 8, Port = 0, Password = "123", UserName = "trung", SyncsTime = 24, SSLEncryptionMode = 0, ConnectionMode = 0, DeleteAfterSync = false, DeleteExpiredMonths = 4, SyncOrderOnly = false };
                dbConn.Insert<FTPConfig>(c);
            }
            #endregion
        }
    }
}