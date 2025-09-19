using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class KhachHangRepository : IKhachHangRepository
    {
        private readonly HungDuyHospitalityContext _context;
        public KhachHangRepository(HungDuyHospitalityContext context)
        {
            _context = context;
        }

        public TblKhachHang GetKhachHangByPhone(string phoneNumber)
        {
            return _context.TblKhachHang
                .AsNoTracking()
                .FirstOrDefault(x => x.SoDienThoai == phoneNumber && !(x.Deleted ?? false));
        }
    }
}
