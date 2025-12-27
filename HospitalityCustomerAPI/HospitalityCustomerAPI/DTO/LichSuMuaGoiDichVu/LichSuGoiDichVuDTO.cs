namespace HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu
{
    public class LichSuGoiDichVuDTO
    {
        public Guid MaLichSuGoiDichVu {get;set;}
        public string TenGoiDichVu {get;set;}
        public DateTime? NgayKichHoat {get;set;}
        public DateTime? NgayHetHan { get; set; }
        public int SoLan {get;set;}
        public int SoLanDaSuDung { get;set;}
        public int ConLai { get;set;}
        public Guid MaBoPhan { get; set; }
        public Guid MaDiemBanHang { get; set; }
    }
}
