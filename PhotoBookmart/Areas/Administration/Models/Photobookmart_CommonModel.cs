using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoBookmart.DataLayer.Models.Products;

namespace PhotoBookmart.Areas.Administration.Models
{
    /// <summary>
    /// to be used on Order management page
    /// </summary>
    public class OrderFilterModel
    {
        public DateTime BetweenDate { get; set; }
        
        public DateTime AndDate { get; set; }

        public string Condition { get; set; }

        public string Search { get; set; }

        /// <summary>
        /// =0: dont care
        /// =1: working
        /// =2: finished
        /// =3: canceled
        /// =4: refund
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// =0: dont care
        /// =1: Working
        /// =2: Waiting
        /// </summary>
        public int StaffStatus { get; set; }
        
        /// <summary>
        /// =0: HTML
        /// =1: Export to Excel
        /// =2: Production Sheet
        /// =3: Shipping
        /// =4: Individual Sheet
        /// </summary>
        public int ResultType { get; set; }

        /// <summary>
        /// =0: Any
        /// =1: Use coupon
        /// =2: Dont use coupon
        /// </summary>
        public int Coupon { get; set; }

        /// <summary>
        /// =-1: any
        /// =0: promo
        /// =1: groupon
        /// </summary>
        public int CouponType { get; set; }

        /// <summary>
        /// Currency code to compare with the order
        /// </summary>
        public string Currency { get; set; }

        public string SortBy
        {
            get;
            set;
        }

        /// <summary>
        /// Filter by product option or not. If empty, then will show all
        /// </summary>
        public List<int> ProductOptions { get; set; }

        public Enum_ShippingType Shipping_Method { get; set; }

        public OrderFilterModel()
        {
            ResultType = 0;
        }
    }

    /// <summary>
    /// Model handle add new message
    /// </summary>
    public class OrderHistory_Add
    {
        public long OrderId { get; set; }
        public string Message { get; set; }
        public bool isPrivate { get; set; }
    }

    /// <summary>
    /// Model to handle Export to Excel
    /// </summary>
    public class Coupon_Instance_ExportExcelModel
    {
        public long CouponId { get; set; }
        public DateTime BetweenDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string IssuedTo { get; set; }
    }
}