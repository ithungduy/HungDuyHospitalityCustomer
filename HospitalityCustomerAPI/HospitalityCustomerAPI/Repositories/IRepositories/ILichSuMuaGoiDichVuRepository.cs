using HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu;
using HospitalityCustomerAPI.Models.HCAEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface ILichSuMuaGoiDichVuRepository
    {
        List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang);
        List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang);
        OpsLichSuMuaGoiDichVu GetById(Guid Ma);
    }
}
