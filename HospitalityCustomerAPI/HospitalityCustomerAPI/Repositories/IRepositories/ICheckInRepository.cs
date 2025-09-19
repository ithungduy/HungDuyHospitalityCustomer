using HospitalityCustomerAPI.DTO.CheckIn;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface ICheckInRepository
    {
        List<CheckInDTO> GetLichSuCheckin(Guid maLichSuGoiDichVu);
    }
}
