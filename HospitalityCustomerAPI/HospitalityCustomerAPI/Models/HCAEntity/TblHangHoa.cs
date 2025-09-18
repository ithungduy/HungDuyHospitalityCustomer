using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class TblHangHoa
{
    public Guid Ma { get; set; }

    public string? MaVach { get; set; }

    public Guid? MaLopHang { get; set; }

    public string? Ten { get; set; }

    public Guid? MaDvt { get; set; }
}
