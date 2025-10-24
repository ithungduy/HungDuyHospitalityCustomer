using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class OpsLichSuMuaGoiDichVu
{
    public Guid Ma { get; set; }

    public Guid? MaPhongBan { get; set; }

    public Guid? MaKhachHang { get; set; }

    public Guid? MaHoaDon { get; set; }

    public Guid? MaHangHoa { get; set; }

    public int? SoLanSuDung { get; set; }

    public int? SoLanDaSuDung { get; set; }

    public int? SoLanConLai { get; set; }

    public decimal? DonGia { get; set; }

    public Guid? NhanVienPt { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public DateTime? NgayKichHoat { get; set; }

    public DateTime? NgayHetHan { get; set; }
}
