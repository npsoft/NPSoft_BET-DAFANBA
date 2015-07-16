using System;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using PhotoBookmart.DataLayer.Models.Users_Management;

namespace PhotoBookmart.DataLayer.Models.Products
{
    [Alias("DoiTuong_BienDong")]
    [Schema("DoiTuong")]
    public partial class DoiTuong_BienDong : BasicModelBase
    {
        [ForeignKey(typeof(DoiTuong), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public long? IDDT { get; set; }
        public int? Order { get; set; }
        public string MaHC { get; set; }
        public Guid? IDDiaChi { get; set; }
        public string TinhTrang { get; set; }
        public string MaLDT { get; set; }
        public DateTime? NgayHuong { get; set; }
        public decimal? HeSo { get; set; }
        public decimal? MucTC { get; set; }
        public string MoTa { get; set; }
        public decimal? MucChenh { get; set; }

        #region Ignore properties
        [Ignore]
        public string MaHC_Ten { get; set; }
        [Ignore]
        public string MaLDT_Ten { get; set; }
        [Ignore]
        public bool CanDelete { get; set; }
        #endregion

        #region Ignore functions
        public bool CheckDelete(ABUserAuth user, string ma_hc, long id)
        {
            if (user == null) { return false; }
            return user.HasRole(RoleEnum.District) && ma_hc.StartsWith(user.MaHC) && Id != id;
        }
        #endregion

        public DoiTuong_BienDong() { }
    }

    public enum Enum_Price_MasterType
    {
        ProductOption,
        ProductShippingPrice,
        Product,
    }

    /// <summary>
    /// Remember to delete the Price when you delete object for we dont know exactly which object connect to the price
    /// </summary>
    [Schema("Products")]
    public partial class Price : ModelBase
    {
        ///// <summary>
        ///// Price we display according to the country
        ///// </summary>
        //public double DisplayPrice { get; set; }
        ///// <summary>
        ///// Price we use to calculate, in RM
        ///// </summary>
        //public double RealPrice { get; set; }

        /// <summary>
        /// Price in this currency
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Vietnam, Malaysia,...
        /// https://developer.paypal.com/docs/classic/api/country_codes/
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// VND, đ, RM, USD, to show to customer
        /// </summary>
        public string CurrencyCode { get; set; }
        ///// <summary>
        ///// This is paypal code
        ///// https://developer.paypal.com/docs/classic/api/currency_codes/
        ///// </summary>
        //public string CurrencyCode_ThereLetters { get; set; }

        /// <summary>
        /// Id of the object which hold the link to this price
        /// </summary>
        public long MasterId { get; set; }

        /// <summary>
        /// Type of the object hold this price
        /// </summary>
        public Enum_Price_MasterType MasterType { get; set; }

        [Ignore]
        public string Master_Name { get; set; }

        public Price()
        {

        }
    }
}
