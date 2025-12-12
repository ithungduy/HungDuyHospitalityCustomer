using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SchDangKyTap
{
    public Guid Ma { get; set; }

    public Guid? MaKhachHang { get; set; }

    public Guid? MaGoiDichVu { get; set; }

    public Guid? MaLichTapLuyen { get; set; }

    public string? GhiChu { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}
