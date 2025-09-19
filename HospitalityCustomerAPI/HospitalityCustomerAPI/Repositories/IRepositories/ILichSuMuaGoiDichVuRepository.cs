using HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface ILichSuMuaGoiDichVuRepository
    {
        List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang);
    }
}
