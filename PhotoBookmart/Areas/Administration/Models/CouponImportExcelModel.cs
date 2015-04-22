using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoBookmart.Areas.Administration.Models
{
    public class CouponImportExcelModel
    {
        public long id { get; set; }
        public string IssuedTo { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}