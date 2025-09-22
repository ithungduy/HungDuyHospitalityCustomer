using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repository
{
    public class TinhThanhRepository : ITinhThanhRepository
    {
        private readonly HungDuyHospitalityContext _context;

        public TinhThanhRepository(HungDuyHospitalityContext context) // Constructor Injection
        {
            _context = context;
        }

        public async Task<IEnumerable<CatTinhThanh>> GetAllAsync()
        {
            var listData = await _context.CatTinhThanh.Where(c => !(c.Deleted ?? false) && c.MaQuocGia == QuocGia.VietNam.GetEnumGuid())
                                 .AsNoTracking()
                                 .OrderBy(t => t.Ten)
                                 .ToListAsync();
            return listData;
        }

        public async Task<IEnumerable<CatTinhThanh>> GetByQuocGiaAsync(string maQuocGia)
        {
            return await _context.CatTinhThanh
                                 .Where(t => t.MaQuocGia.ToString() == maQuocGia && !(t.Deleted ?? false))
                                 .AsNoTracking()
                                 .OrderBy(t => t.Ten)
                                 .ToListAsync();
        }
    }
}
