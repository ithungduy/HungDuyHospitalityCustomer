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
                        SoLan = t.SoLanSuDung ?? 0,
                        SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                        ConLai = t.SoLanConLai ?? 0,
                    }).ToList();
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang)
        {
            List<LichSuGoiDichVuDTO> goiDichVu = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                                                  join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                                                  where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                  && (t.SoLanSuDung ?? 0) - (t.SoLanDaSuDung ?? 0) > 0
                                                  select new LichSuGoiDichVuDTO
                                                  {
                                                      MaLichSuGoiDichVu = t.Ma,
                                                      TenGoiDichVu = dv.Ten,
                                                      NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
                                                      NgayHetHan = t.NgayHetHan,
                                                      SoLan = t.SoLanSuDung ?? 0,
                                                      SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                                      ConLai = t.SoLanConLai ?? 0,
                                                  }).ToList();

            List<LichSuGoiDichVuDTO> goiDichVuGiaDinh = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                                                         join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                                                         join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                                                         where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                         && (gdv.SoLanSuDung ?? 0) - (gdv.SoLanDaSuDung ?? 0) > 0
                                                         select new LichSuGoiDichVuDTO
                                                         {
                                                             MaLichSuGoiDichVu = gdv.Ma,
                                                             TenGoiDichVu = dv.Ten,
                                                             NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
                                                             NgayHetHan = gdv.NgayHetHan,
                                                             SoLan = gdv.SoLanSuDung ?? 0,
                                                             SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
                                                             ConLai = gdv.SoLanConLai ?? 0,
                                                         }).ToList();

            goiDichVu.AddRange(goiDichVuGiaDinh);

            return goiDichVu;
        }
    }
}
