using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SysAppVersion
{
    public Guid Id { get; set; }

    public string? AndroidVer { get; set; }

    public string? IosVer { get; set; }

    public string? Description { get; set; }

    public DateTime? LastUpdatedTime { get; set; }

    public string? LastUpdatedBy { get; set; }

    public string? Appver { get; set; }
}
