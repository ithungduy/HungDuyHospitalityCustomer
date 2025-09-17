using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class NwsTinTuc
{
    public int Ma { get; set; }

    public int? MaLoai { get; set; }

    public string? TenLoai { get; set; }

    public string? Title { get; set; }

    public string? Link { get; set; }

    public string? MoTaNgan { get; set; }

    public string? HinhAnh { get; set; }

    public bool? TinNoiBat { get; set; }

    public bool? Status { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}
