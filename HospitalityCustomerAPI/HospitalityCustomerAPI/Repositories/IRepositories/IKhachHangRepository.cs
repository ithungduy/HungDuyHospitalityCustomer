using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface IKhachHangRepository
    {
        TblKhachHang GetKhachHangByPhone(string phoneNumber);
    }
}
