using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoBookmart.Areas.Administration.Models
{
    public class BienDong_CatChetModel
    {
        public long Id { get; set; }
        public DateTime NgayBienDong { get; set; }

        public BienDong_CatChetModel() { }
    }

    public class BienDong_DungTroCapModel
    {
        public long Id { get; set; }
        public DateTime NgayBienDong { get; set; }

        public BienDong_DungTroCapModel() { }
    }

    public class BienDong_ChuyenDiaBanModel
    {
        public long Id { get; set; }
        public string MaHC { get; set; }
        public Guid? IDDiaChi { get; set; }
        public DateTime NgayBienDong { get; set; }

        public BienDong_ChuyenDiaBanModel() { }
    }

    public class BienDong_ThayDoiLoaiDoiTuong
    {
        public long Id { get; set; }
        public DateTime NgayBienDong { get; set; }

        public BienDong_ThayDoiLoaiDoiTuong() { }
    }
}
