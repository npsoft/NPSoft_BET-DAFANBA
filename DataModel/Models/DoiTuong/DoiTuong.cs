using System;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using PhotoBookmart.DataLayer.Models.ExtraShipping;
using PhotoBookmart.DataLayer.Models.Reports;
using PhotoBookmart.DataLayer.Models.System;

namespace PhotoBookmart.DataLayer.Models.Products
{
    [Alias("DoiTuong")]
    [Schema("DoiTuong")]
    public partial class DoiTuong : ModelBase
    {
        public string MaHC { get; set; }
        public Guid? IDDiaChi { get; set; }
        [ForeignKey(typeof(DanhMuc_DanToc), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public long? MaDanToc { get; set; }
        [ForeignKey(typeof(DanhMuc_TinhTrangDT), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public long? TinhTrang { get; set; }
        public Guid IDDT { get; set; }
        public int Order { get; set; }
        public string HoTen { get; set; }
        public string NgaySinh { get; set; }
        public string ThangSinh { get; set; }
        public string NamSinh { get; set; }
        public string GioiTinh { get; set; }
        public string CMTND { get; set; }
        public DateTime? NgayCap { get; set; }
        public string NoiCap { get; set; }
        public string NguyenQuan { get; set; }
        public string TruQuan { get; set; }

        #region TODO: #1
        public bool? isBHYT { get; set; }
        public bool? isHoNgheo { get; set; }
        public bool? isKhuyetTat { get; set; }
        public Guid? DangKT { get; set; }
        public Guid? MucDoKT { get; set; }
        public string MaLDT { get; set; }
        public decimal? MucTC { get; set; }
        public DateTime? NgayHuong { get; set; }
        #endregion

        #region TODO: #2
        public string SoQD { get; set; }
        public DateTime? NgayQD { get; set; }
        public string GhiChu { get; set; }
        #endregion

        #region TODO: Ignore
        [Ignore]
        public List<DoiTuong_LoaiDoiTuong_CT> MaLDT_Details { get; set; }
        [Ignore]
        public bool IsContinue { get; set; }
        #endregion

        public DoiTuong()
        {
            MaLDT_Details = new List<DoiTuong_LoaiDoiTuong_CT>();
        }
    }

    [Alias("Products")]
    [Schema("Products")]
    public partial class Product : ModelBase
    {
        public bool Status { get; set; }

        public string Name { get; set; }

        public List<string> Tag { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public bool PriceDontShow { get; set; }

        public bool PriceCallForPrice { get; set; }

        public bool isFreeShip { get; set; }

        public bool ShowOnHomepage { get; set; }

        public string SeoName { get; set; }

        // thứ tự sắp xếp
        public int Order { get; set; }

        public string Size { get; set; }

        public int Pages { get; set; }

        public string Paper { get; set; }

        public string Orientation { get; set; }

        public long CatId { get; set; }
        [Ignore]
        public string Category_Name { get; set; }

        /// <summary>
        ///  This is the pID from the My Photo Creation. We follow them in order to capture the product 
        /// </summary>
        public int MyPhotoCreationId { get; set; }

        /// <summary>
        /// Weight of the product, in grams
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// If yes, then when customer submit order, they can select cover material
        /// </summary>
        public bool IsAllowCoverMaterialSelect { get; set; }

        #region Front End use
        /// <summary>
        /// For Front End use only
        /// Calculate the shipping cost on server before send it
        /// </summary>
        [Ignore]
        public List<Price> ShippingPrice { get; set; }

        /// <summary>
        /// Product price, to be shown on order page
        /// </summary>
        [Ignore]
        public double Price { get; set; }
        #endregion

        public Product()
        {
            Id = 0;

            CreatedBy = 0;

            CreatedOn = DateTime.Now;

            Order = 0;

            Tag = new List<string>();

            SeoName = "";

            Pages = 0;

            ShippingPrice = new List<Price>();
        }

        public List<Product_Images> GetImages()
        {
            //var images = this.InternalService.Db.Where<Product_Images>(m => m.Status && m.ProductId == this.Id);
            var images = Db.Where<Product_Images>(m => m.Status && m.ProductId == this.Id);
            Db.Close();
            return images;
        }

        public Price getPrice(Enum_Price_MasterType type, string countrycode)
        {
            if (string.IsNullOrEmpty(countrycode))
            {
                countrycode = "MY";
            }

            countrycode = countrycode.ToUpper().Trim();

            var price = base.getPrice(this.Id, type, countrycode);
            Price ret = null;
            if (price.Count > 0)
            {
                if (string.IsNullOrEmpty(countrycode))
                {
                    // find RM, Malaysia
                    ret = price.Where(x => x.CountryCode == "MY").FirstOrDefault();
                    if (ret == null)
                    {
                        ret = price.FirstOrDefault();
                    }
                }
                else
                {
                    ret = price.FirstOrDefault();
                }
            }
            else
            {
                ret = new Price();
            }
            Db.Close();
            return ret;
        }
    }
}
