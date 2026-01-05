using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class TblPhongBan
{
    public Guid Ma { get; set; }

    public Guid? MaChiNhanh { get; set; }

    public string? Code { get; set; }

    public string? Ten { get; set; }

    public string? MoTa { get; set; }

    public Guid? MaBan { get; set; }

    public bool? CoSoQuy { get; set; }

    public bool? Ban { get; set; }

    public bool? Status { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public int? MaNganhVoucher { get; set; }

    public int? MaNhomBoPhan { get; set; }

    public bool? PhatHanhHoaDonTuDong { get; set; }
}
