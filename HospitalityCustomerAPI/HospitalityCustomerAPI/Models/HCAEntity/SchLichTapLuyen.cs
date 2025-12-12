using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SchLichTapLuyen
{
    public Guid Ma { get; set; }

    public Guid? MaBoPhan { get; set; }

    public string? TenBoPhan { get; set; }

    public Guid? MaPhongTap { get; set; }

    public string? TenPhongTap { get; set; }

    public Guid? MaHuanLuyenVien { get; set; }

    public string? TenHuanLuyenVien { get; set; }

    public DateTime? NgayTapLuyen { get; set; }

    public bool? BuoiSang { get; set; }

    public bool? BuoiChieu { get; set; }

    public TimeOnly? TuGio { get; set; }

    public TimeOnly? DenGio { get; set; }

    public string? NoiDung { get; set; }

    public int? SoHocVien { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}
