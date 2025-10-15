using System;
using System.Collections.Generic;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class SysUser
{
    public Guid Ma { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? SoDienThoai { get; set; }

    public int? GioiTinh { get; set; }

    public string? HinhAnh { get; set; }

    public string? Cccd { get; set; }

    public string? Fcm { get; set; }

    public bool? Reminder { get; set; }

    public bool? Status { get; set; }

    public bool? LdapLogin { get; set; }

    public int? Diem { get; set; }

    public Guid? UserCreated { get; set; }

    public DateTime? CreatedDate { get; set; }

    public Guid? UserModified { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Deleted { get; set; }

    public Guid? UserDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public Guid? MaKhachHang { get; set; }

    public string? CodeKhachHang { get; set; }

    public string? Token { get; set; }

    public DateTime? NgaySinh { get; set; }

    public Guid? MaQuocGia { get; set; }

    public Guid? MaTinh { get; set; }

    public Guid? MaPhuongXa { get; set; }

    public string? SoNha { get; set; }

    public Guid? QuocTich { get; set; }

    public string? HoChieu { get; set; }

    public Guid? MaDanToc { get; set; }

    public string? TienSuBenhLy { get; set; }
}
