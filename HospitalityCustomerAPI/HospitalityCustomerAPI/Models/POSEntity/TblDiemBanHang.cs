using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class TblDiemBanHang
{
    public Guid Ma { get; set; }

    public Guid MaChiNhanh { get; set; }

    public string? Code { get; set; }

    public string? Ten { get; set; }

    public Guid? MaPhongBan { get; set; }

    public Guid? MaKho { get; set; }

    public int? SoLanIn { get; set; }

    public bool? Status { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? IpOpenDoor { get; set; }

    public string? ControlPin { get; set; }
}
