using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class LichSuMuaGoiDichVuPOSRepository : ILichSuMuaGoiDichVuPOSRepository
    {
        public readonly HungDuyHospitalityContext _context;
        public LichSuMuaGoiDichVuPOSRepository(HungDuyHospitalityContext context)
        {
            _context = context;
        }
        public OpsLichSuMuaGoiDichVu GetById(Guid Ma)
        {
            return _context.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefault(x => x.Ma == Ma && !(x.Deleted ?? false));
        }
    }
}
