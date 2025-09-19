using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface IDiemBanHangPOSRepository
    {
        TblDiemBanHang GetById(Guid Ma);
    }
}
