using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repository.IRepository
{
    public interface IPhuongXaRepository
    {
        Task<IEnumerable<CatPhuongXa>> GetAllAsync();
        Task<IEnumerable<CatPhuongXa>> GetByTinhThanhAsync(string maTinhThanh);

    }
}
