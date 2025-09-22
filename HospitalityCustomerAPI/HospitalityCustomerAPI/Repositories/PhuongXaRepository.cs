using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repository
{
    public class PhuongXaRepository : IPhuongXaRepository
    {
        private readonly HungDuyHospitalityContext _context;

        public PhuongXaRepository(HungDuyHospitalityContext context) // Constructor Injection
        {
            _context = context;
        }

        public async Task<IEnumerable<CatPhuongXa>> GetAllAsync()
        {
            var listData = await _context.CatPhuongXa.Where(c => !(c.Deleted ?? false))
                                 .AsNoTracking()
                                 .OrderBy(t => t.Ten)
                                 .ToListAsync();
            return listData;
        }

        public async Task<IEnumerable<CatPhuongXa>> GetByTinhThanhAsync(string maTinhThanh)
        {
            return await _context.CatPhuongXa
                                 .Where(p => p.MaTinhThanh.ToString() == maTinhThanh && !(p.Deleted ?? false))
                                 .AsNoTracking()
                                 .OrderBy(p => p.Ten)
                                 .ToListAsync();
        }
    }
}
