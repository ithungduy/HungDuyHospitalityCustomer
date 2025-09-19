using HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class LichSuMuaGoiDichVuRepository : ILichSuMuaGoiDichVuRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;
        public LichSuMuaGoiDichVuRepository(HungDuyHospitalityCustomerContext context)
        {
            _context = context;
        }

        public OpsLichSuMuaGoiDichVu GetById(Guid Ma)
        {
            return _context.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefault(x => x.Ma == Ma && !(x.Deleted ?? false));
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang)
        {
            return (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                    join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                    where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                    select new LichSuGoiDichVuDTO
                    {
                        MaLichSuGoiDichVu = t.Ma,
                        TenGoiDichVu = dv.Ten,
                        NgayKichHoat = t.CreatedDate,
                        SoLan = t.SoLanDaSuDung ?? 0,
                        SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                        ConLai = t.SoLanConLai ?? 0,
                    }).ToList();
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang)
        {
            return (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                    join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                    where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                    && (t.SoLanSuDung ?? 0) - (t.SoLanDaSuDung ?? 0) > 0
                    select new LichSuGoiDichVuDTO
                    {
                        MaLichSuGoiDichVu = t.Ma,
                        TenGoiDichVu = dv.Ten,
                        NgayKichHoat = t.CreatedDate,
                        SoLan = t.SoLanDaSuDung ?? 0,
                        SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                        ConLai = t.SoLanConLai ?? 0,
                    }).ToList();
        }
    }
}
