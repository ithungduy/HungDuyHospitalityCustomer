using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repository
{
    public class QuocGiaRepository : IQuocGiaRepository
    {
        private readonly HungDuyHospitalityContext _context;

        public QuocGiaRepository(HungDuyHospitalityContext context) // Constructor Injection
        {
            _context = context;
        }

        public async Task<IEnumerable<CatQuocGia>> GetAllAsync()
        {
            var listData = await _context.CatQuocGia.Where(c => !(c.Deleted ?? false))
                                  .AsNoTracking()
                                  .OrderBy(t => t.Ten)
                                  .ToListAsync();
            return listData;
        }
    }
}
