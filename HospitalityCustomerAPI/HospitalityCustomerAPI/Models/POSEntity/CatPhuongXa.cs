using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class CatPhuongXa
{
    public Guid Ma { get; set; }

    public string? Code { get; set; }

    public string? Ten { get; set; }

    public Guid? MaTinhThanh { get; set; }

    public Guid? MaHuyen { get; set; }

    public bool? Status { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public int? Version { get; set; }
}
