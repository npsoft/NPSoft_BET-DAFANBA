using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;

namespace ABSoft.Photobookmart.FTPSync.Models
{
    public partial class FTPConfig
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }

        public string FTPHost { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FTPDefaultPath { get; set; }

        /// <summary>
        /// =0: None
        /// =1: Explicit is TLS, 
        //  =2  Implicit is SSL.
        /// </summary>
        public int SSLEncryptionMode { get; set; }

        public int Port { get; set; }

        public int SyncsTime { get; set; }

        public string LocalPath { get; set; }

        public int ServerTimeZone { get; set; }

        public int LocalTimeZone { get; set; }

        public bool DeleteAfterSync { get; set; }

        public bool SyncOrderOnly { get; set; }

        public int DeleteExpiredMonths { get; set; }

        /// <summary>
        /// =0: Auto
        /// =1: Active
        /// =2: Passive
        /// </summary>
        public int ConnectionMode { get; set; }
        
        public FTPConfig() {
            
        }
    }
}