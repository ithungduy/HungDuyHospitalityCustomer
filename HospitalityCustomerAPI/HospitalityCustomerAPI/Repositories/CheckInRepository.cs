using HospitalityCustomerAPI.DTO.CheckIn;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class CheckInRepository : ICheckInRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;

        public CheckInRepository(HungDuyHospitalityCustomerContext context)
        {
            _context = context;
        }

        public List<CheckInDTO> GetLichSuCheckin(Guid maLichSuGoiDichVu)
        {
            var listLishSuCheckIn = (from t in _context.OpsCheckIn.AsNoTracking()
                                     join g in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals g.Ma
                                     join dv in _context.TblHangHoa.AsNoTracking() on g.MaHangHoa equals dv.Ma
                                     where t.MaLichSuGoiDichVu == maLichSuGoiDichVu && !(t.Deleted ?? false)
                                     select new CheckInDTO
                                     {
                                         MaGoiDichVu = t.Ma,
                                         TenGoiDichVu = dv.Ten,
                                         Ngay = t.CreatedDate,
                                     }).ToList();

            return listLishSuCheckIn;
        }
    }
}
