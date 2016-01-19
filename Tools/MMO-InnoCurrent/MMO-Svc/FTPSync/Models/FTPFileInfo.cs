using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;

namespace ABSoft.Photobookmart.FTPSync.Models
{
    public partial class FTPFileCheck
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }

        public string FullName { get; set; }

        public string Hash { get; set; }

        public FTPFileCheck() { }
    }
}