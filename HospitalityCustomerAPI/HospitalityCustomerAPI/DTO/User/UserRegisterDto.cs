namespace HospitalityCustomerAPI.DTO.User
{

    public class UserRegisterDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? HinhAnh { get; set; }
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public int GioiTinh { get; set; }
        public Guid MaQuocGia { get; set; }
        public Guid MaTinh { get; set; }
        public Guid MaHuyen { get; set; }
        public Guid MaPhuongXa { get; set; }
        public string? SoNha { get; set; }
        public Guid QuocTich { get; set; }
        public Guid MaDanToc { get; set; }
        public string? Otp { get; set; }
        public string? HoChieu { get; set; }
    }

    public class UserUpdateDto
    {
        public string Username { get; set; } = "";
        public string? HinhAnh { get; set; }
        public string? HoTen { get; set; }
        public string? NgaySinh { get; set; }
        public int GioiTinh { get; set; }
        public Guid MaQuocGia { get; set; }
        public Guid MaTinh { get; set; }
        public Guid MaHuyen { get; set; }
        public Guid MaPhuongXa { get; set; }
        public string? SoNha { get; set; }
        public Guid QuocTich { get; set; }
        public Guid MaDanToc { get; set; }
        public bool Status { get; set; }
        public string? HoChieu { get; set; }

    }

    public class ResetPasswordDto
    {
        public string Username { get; set; } = "";
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }

    public class ResetForgotPasswordDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Otp { get; set; } = "";
    }


    public class CheckinDto
    {
        public string MaDiemBanHang { get; set; }
        public Guid MaLichSuGoiDichVu { get; set; }
        public Guid MaKhachHang { get; set; }
        public string? GpioAlias { get; set; } // ví dụ: "gym_door_2"
    }
}
