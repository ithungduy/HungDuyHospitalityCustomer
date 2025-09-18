using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class TblKhachHang
{
    public Guid Ma { get; set; }

    public string? Code { get; set; }

    public string? Ten { get; set; }

    public string? Cccd { get; set; }

    public string? SoDienThoai { get; set; }

    public int? GioiTinh { get; set; }

    public DateTime? NgaySinh { get; set; }

    public string? DiaChi { get; set; }

    public Guid? MaLoaiKhachHang { get; set; }

    public bool? Status { get; set; }

    public bool? ChoPhepCongNo { get; set; }

    public decimal? HanMucCongNo { get; set; }

    public int? Diem { get; set; }

    public string? Msnv { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }
}
