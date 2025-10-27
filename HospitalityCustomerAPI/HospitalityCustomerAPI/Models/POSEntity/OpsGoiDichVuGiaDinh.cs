using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class OpsGoiDichVuGiaDinh
{
    public Guid Ma { get; set; }

    public Guid? MaLichSuGoiDichVu { get; set; }

    public Guid? MaKhachHang { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}
