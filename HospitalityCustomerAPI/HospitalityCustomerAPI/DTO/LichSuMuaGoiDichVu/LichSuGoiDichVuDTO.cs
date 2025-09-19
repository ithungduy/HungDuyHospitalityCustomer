namespace HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu
{
    public class LichSuGoiDichVuDTO
    {
        public Guid MaLichSuGoiDichVu {get;set;}
        public string TenGoiDichVu {get;set;}
        public DateTime? NgayKichHoat {get;set;}
        public int SoLan {get;set;}
        public int SoLanDaSuDung { get;set;}
        public int ConLai { get;set;}
    }
}
