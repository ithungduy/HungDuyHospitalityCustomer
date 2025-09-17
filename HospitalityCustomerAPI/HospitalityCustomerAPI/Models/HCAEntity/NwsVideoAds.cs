using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class NwsVideoAds
{
    public int Ma { get; set; }

    public string? Ten { get; set; }

    public string? Link { get; set; }

    public string? Thumbnail { get; set; }

    public bool? Status { get; set; }

    public int? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public int? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? LinkWeb { get; set; }
}
