using System;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;

namespace PhotoBookmart.DataLayer.Models.Products
{
    [Schema("Products")]
    public partial class OptionInProduct : ModelBase
    {
        [Default(0)]
        [ForeignKey(typeof(Product), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public long ProductId { get; set; }
        [Ignore]
        public string Product_Name { get; set; }
        [Ignore]
        public string Option_Name { get; set; }

        [Default(0)]
        [ForeignKey(typeof(Product_Option), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public long ProductOptionId { get; set; }

        /// <summary>
        /// If isRequire then can not remove
        /// </summary>
        public bool isRequire { get; set; }

        /// <summary>
        /// Default quantity the admin set this option for the product
        /// </summary>
        public int DefaultQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public int MinQuantity { get; set; }

        public bool CanApplyCoupon { get; set; }

        public OptionInProduct()
        {
            CreatedOn = DateTime.Now;
        }
    }
}
