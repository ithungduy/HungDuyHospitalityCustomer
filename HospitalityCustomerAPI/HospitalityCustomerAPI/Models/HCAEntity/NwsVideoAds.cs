using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class NwsVideoAds
{
    public Guid Ma { get; set; }

    public string? Ten { get; set; }

    public string? Link { get; set; }

    public string? Thumbnail { get; set; }

    public bool? Status { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string? LinkWeb { get; set; }
}
