using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class DiemBanHangPOSRepository : IDiemBanHangPOSRepository
    {
        private readonly HungDuyHospitalityContext _context;

        public DiemBanHangPOSRepository(HungDuyHospitalityContext context)
        {
            _context = context;
        }

        public TblDiemBanHang GetById(Guid Ma)
        {
            return _context.TblDiemBanHang.AsNoTracking().FirstOrDefault(x => x.Ma == Ma && !(x.Deleted ?? false));
        }
    }
}
