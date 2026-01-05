using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class OpsCheckIn
{
    public Guid Ma { get; set; }

    public Guid? MaChiNhanh { get; set; }

    public Guid? MaPhongBan { get; set; }

    public Guid? MaDiemBanHang { get; set; }

    public Guid? MaKhachHang { get; set; }

    public Guid? MaNhanVien { get; set; }

    public DateTime? NgayCheckIn { get; set; }

    public int? Diem { get; set; }

    public Guid? MaNhanVienPhuTrach { get; set; }

    public Guid? MaPhuCap { get; set; }

    public decimal? TienPhuCap { get; set; }

    public Guid? MaLichSuGoiDichVu { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public Guid? MaCheckInPos { get; set; }

    public bool? KhongDenTap { get; set; }
}
