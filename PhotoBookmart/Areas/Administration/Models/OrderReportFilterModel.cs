using System;

namespace PhotoBookmart.Areas.Administration.Models
{
    public class OrderReportFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? MaxRows { get; set; }
    }
}