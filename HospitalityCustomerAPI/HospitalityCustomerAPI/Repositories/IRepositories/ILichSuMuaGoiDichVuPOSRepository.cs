using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface ILichSuMuaGoiDichVuPOSRepository
    {
        OpsLichSuMuaGoiDichVu GetById(Guid Ma);
    }
}
