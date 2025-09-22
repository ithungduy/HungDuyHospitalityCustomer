using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repository.IRepository
{
    public interface ITinhThanhRepository
    {
        Task<IEnumerable<CatTinhThanh>> GetAllAsync();
        Task<IEnumerable<CatTinhThanh>> GetByQuocGiaAsync(string maQuocGia);

    }
}
