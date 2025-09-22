using HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repository.IRepository
{
    public interface IQuocGiaRepository
    {
        Task<IEnumerable<CatQuocGia>> GetAllAsync();
    }
}
