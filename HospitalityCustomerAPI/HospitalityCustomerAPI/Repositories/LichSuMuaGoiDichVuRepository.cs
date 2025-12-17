using HospitalityCustomerAPI.Common;
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


        // dùng cho check lịch sử đăng kí
        public List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang)
        {
            List<LichSuGoiDichVuDTO> goiDichVu = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                                                  join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                                                  where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                  //&& (t.SoLanSuDung ?? 0) - (t.SoLanDaSuDung ?? 0) > 0
                                                  select new LichSuGoiDichVuDTO
                                                  {
                                                      MaLichSuGoiDichVu = t.Ma,
                                                      TenGoiDichVu = dv.Ten,
                                                      NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
                                                      NgayHetHan = t.NgayHetHan,
                                                      SoLan = t.SoLanSuDung ?? 0,
                                                      SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                                      ConLai = t.SoLanConLai ?? 0,
                                                      MaBoPhan = t.MaPhongBan ?? Utility.defaultUID,
                                                  }).ToList();

            List<LichSuGoiDichVuDTO> goiDichVuGiaDinh = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                                                         join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                                                         join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                                                         where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                         //&& (gdv.SoLanSuDung ?? 0) - (gdv.SoLanDaSuDung ?? 0) > 0
                                                         select new LichSuGoiDichVuDTO
                                                         {
                                                             MaLichSuGoiDichVu = gdv.Ma,
                                                             TenGoiDichVu = dv.Ten,
                                                             NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
                                                             NgayHetHan = gdv.NgayHetHan,
                                                             SoLan = gdv.SoLanSuDung ?? 0,
                                                             SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
                                                             ConLai = gdv.SoLanConLai ?? 0,
                                                             MaBoPhan = gdv.MaPhongBan ?? Utility.defaultUID,
                                                         }).ToList();

            goiDichVu.AddRange(goiDichVuGiaDinh);

            return goiDichVu;
        }       


        public List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang)
        {
            var today = DateTime.Now.Date;
            List<LichSuGoiDichVuDTO> goiDichVu = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                                                  join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                                                  where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                  && (t.SoLanSuDung ?? 0) - (t.SoLanDaSuDung ?? 0) > 0
                                                  && (t.NgayKichHoat == null || t.NgayKichHoat <= today)
                                                  && (t.NgayHetHan != null && t.NgayHetHan >= today)
                                                  select new LichSuGoiDichVuDTO
                                                  {
                                                      MaLichSuGoiDichVu = t.Ma,
                                                      TenGoiDichVu = dv.Ten,
                                                      NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
                                                      NgayHetHan = t.NgayHetHan,
                                                      SoLan = t.SoLanSuDung ?? 0,
                                                      SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                                      ConLai = t.SoLanConLai ?? 0,
                                                      MaBoPhan = t.MaPhongBan ?? Guid.Empty
                                                  }).ToList();

            List<LichSuGoiDichVuDTO> goiDichVuGiaDinh = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                                                         join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                                                         join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                                                         where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                                         && (gdv.SoLanSuDung ?? 0) - (gdv.SoLanDaSuDung ?? 0) > 0
                                                         && (gdv.NgayKichHoat == null || gdv.NgayKichHoat <= today)
                                                         && (gdv.NgayHetHan != null && gdv.NgayHetHan >= today)
                                                         select new LichSuGoiDichVuDTO
                                                         {
                                                             MaLichSuGoiDichVu = gdv.Ma,
                                                             TenGoiDichVu = dv.Ten,
                                                             NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
                                                             NgayHetHan = gdv.NgayHetHan,
                                                             SoLan = gdv.SoLanSuDung ?? 0,
                                                             SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
                                                             ConLai = gdv.SoLanConLai ?? 0,
                                                             MaBoPhan = gdv.MaPhongBan ?? Guid.Empty
                                                         }).ToList();

            goiDichVu.AddRange(goiDichVuGiaDinh);

            return goiDichVu;
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVuByBoPhan(Guid MaKhachHang, Guid MaBoPhan)
        {
            var query = from ls in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() // Thêm AsNoTracking cho tối ưu
                                                                                 // 1. Sửa join: Dùng MaHangHoa và bảng TblHangHoa
                        join g in _context.TblHangHoa.AsNoTracking() on ls.MaHangHoa equals g.Ma
                        where ls.MaKhachHang == MaKhachHang
                           && ls.MaPhongBan == MaBoPhan
                           && !(ls.Deleted ?? false)

                           // 2. Sửa logic check kích hoạt: Dựa vào NgayKichHoat != null
                           && ls.NgayKichHoat != null

                           // 3. Logic hiển thị gói khả dụng: Còn lượt
                           && ((ls.SoLanSuDung ?? 0) - (ls.SoLanDaSuDung ?? 0) > 0)

                           // 4. Logic hạn sử dụng: Chưa hết hạn hoặc không có hạn
                           && (ls.NgayHetHan == null || ls.NgayHetHan.Value.Date >= DateTime.Now.Date)

                        orderby ls.NgayHetHan ascending
                        select new LichSuGoiDichVuDTO
                        {
                            MaLichSuGoiDichVu = ls.Ma,
                            TenGoiDichVu = g.Ten,
                            NgayKichHoat = ls.NgayKichHoat,
                            NgayHetHan = ls.NgayHetHan,
                            SoLan = ls.SoLanSuDung ?? 0,
                            SoLanDaSuDung = ls.SoLanDaSuDung ?? 0,                            
                            ConLai = ls.SoLanConLai ?? 0,                            
                            MaBoPhan = ls.MaPhongBan ?? Guid.Empty
                        };

            return query.ToList();
        }
    }
}
