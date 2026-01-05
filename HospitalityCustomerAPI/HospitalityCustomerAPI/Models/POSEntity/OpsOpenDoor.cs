using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class OpsOpenDoor
{
    public Guid Ma { get; set; }

    public Guid? MaDiemBanHang { get; set; }

    public Guid? MaKhachHang { get; set; }

    public Guid? MaGoiDichVu { get; set; }

    public DateTime? CreatedDate { get; set; }
}
