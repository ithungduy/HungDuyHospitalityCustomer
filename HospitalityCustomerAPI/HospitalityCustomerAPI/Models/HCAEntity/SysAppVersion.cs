using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SysAppVersion
{
    public string Appver { get; set; } = null!;

    public string? Description { get; set; }
}
