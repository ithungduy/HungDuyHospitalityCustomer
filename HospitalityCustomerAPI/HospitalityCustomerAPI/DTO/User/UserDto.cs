namespace HospitalityCustomerAPI.DTO.User
{
    public class UserDto
    {
        public class UserLoginDto
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string DeviceID { get; set; } = "";
            public string? FCM { get; set; }
        }

        public class UserResponseDto
        {
            public Guid? Ma { get; set; }
            public string Username { get; set; } = "";
            public string AppVersion { get; set; } = "";
            public string Token { get; set; } = "";
            public string DeviceId { get; set; } = "";
        }

        public class UserInfoDto
        {
            public Guid MaUser { get; set; }
            public string Username { get; set; } = "";
            public string AppVersion { get; set; } = "";
            public string Token { get; set; } = "";
            public string DeviceId { get; set; } = "";
            public string? HinhAnh { get; set; }
            public string? FullName { get; set; }
            public string? NgaySinh { get; set; }
            public int? GioiTinh { get; set; }
            public Guid? MaQuocGia { get; set; }
            public Guid? MaTinh { get; set; }
            public Guid? MaHuyen { get; set; }
            public Guid? MaPhuongXa { get; set; }
            public string? SoNha { get; set; }
            public Guid? QuocTich { get; set; }
            public Guid? MaDanToc { get; set; }
            public string? HoChieu { get; set; }

            public Guid MaKhachHang { get; set; }
            public string? Ten { get; set; }
            public string? SoDienThoai { get; set; }
            public string? DiaChi { get; set; }

            public string? TienSuBenhLy { get; set; }
            public string? DongYTuanThuNoiQuy { get; set; }
            public string? ChoPhepSuDungThongTinCaNhan { get; set; }
        }
    }
}
