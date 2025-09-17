using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SysSmsOtp
{
    public Guid Ma { get; set; }

    public string? Sdt { get; set; }

    public string? Otp { get; set; }

    public decimal? GiaTriGiaoDich { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? NumberRequest { get; set; }
}
