using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

using HCAEntity = HospitalityCustomerAPI.Models.HCAEntity;
using POSEntity = HospitalityCustomerAPI.Models.POSEntity;

namespace HospitalityCustomerAPI.Repositories
{
    public class LichSuMuaGoiDichVuRepository : ILichSuMuaGoiDichVuRepository
    {
        private readonly HCAEntity.HungDuyHospitalityCustomerContext _context;
        private readonly POSEntity.HungDuyHospitalityContext _posContext;

        public LichSuMuaGoiDichVuRepository(HCAEntity.HungDuyHospitalityCustomerContext context,
            POSEntity.HungDuyHospitalityContext posContext)
        {
            _context = context;
            _posContext = posContext;
        }

        public HCAEntity.OpsLichSuMuaGoiDichVu GetById(Guid Ma)
        {
            return _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                .FirstOrDefault(x => x.Ma == Ma && !(x.Deleted ?? false));
        }

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


        //// dùng cho check lịch sử đăng kí
        //public List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang)
        //{
        //    // Bước 1: Query dữ liệu thô từ HCA Context
        //    var rawData = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
        //                   join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
        //                   where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
        //                   select new LichSuGoiDichVuDTO
        //                   {
        //                       MaLichSuGoiDichVu = t.Ma,
        //                       TenGoiDichVu = dv.Ten,
        //                       NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
        //                       NgayHetHan = t.NgayHetHan,
        //                       SoLan = t.SoLanSuDung ?? 0,
        //                       SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
        //                       ConLai = t.SoLanConLai ?? 0,
        //                       MaBoPhan = t.MaPhongBan ?? Utility.defaultUID, // Tạm lấy MaPhongBan nội bộ
        //                   }).ToList();

        //    // Lấy thêm gói gia đình (giữ nguyên logic cũ của bạn)
        //    var rawDataGiaDinh = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
        //                          join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
        //                          join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
        //                          where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
        //                          select new LichSuGoiDichVuDTO
        //                          {
        //                              MaLichSuGoiDichVu = gdv.Ma,
        //                              TenGoiDichVu = dv.Ten,
        //                              NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
        //                              NgayHetHan = gdv.NgayHetHan,
        //                              SoLan = gdv.SoLanSuDung ?? 0,
        //                              SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
        //                              ConLai = gdv.SoLanConLai ?? 0,
        //                              MaBoPhan = gdv.MaPhongBan ?? Utility.defaultUID,
        //                          }).ToList();

        //    rawData.AddRange(rawDataGiaDinh);

        //    // Bước 2: Xử lý MAPPING khác DB (Sửa lỗi cú pháp SQL '$')
        //    if (rawData.Any())
        //    {
        //        // Lấy danh sách ID phòng ban từ dữ liệu thô
        //        var listPhongBanIds = rawData.Select(x => x.MaBoPhan).Distinct().ToList();

        //        // --- ĐOẠN CODE ĐÃ SỬA ---

        //        // 1. Tải toàn bộ danh mục Điểm Bán Hàng có liên kết về RAM
        //        // (SQL query này rất đơn giản, chạy tốt trên mọi phiên bản SQL Server)
        //        var allPosLinks = _posContext.TblDiemBanHang.AsNoTracking()
        //            .Where(x => x.MaPhongBan != null)
        //            .Select(x => new { x.Ma, x.MaPhongBan })
        //            .ToList(); // <--- Execute SQL ngay tại đây

        //        // 2. Thực hiện lọc danh sách trên RAM (Client-side evaluation)
        //        // Việc so sánh listPhongBanIds.Contains lúc này do C# xử lý, không bắn câu lệnh xuống DB nữa
        //        var mapDiemBanHang = allPosLinks
        //            .Where(x => listPhongBanIds.Contains(x.MaPhongBan!.Value))
        //            .ToList();

        //        // 3. Cập nhật lại MaBoPhan thành MaDiemBanHang (Tráo ID)
        //        foreach (var item in rawData)
        //        {
        //            var pos = mapDiemBanHang.FirstOrDefault(p => p.MaPhongBan == item.MaBoPhan);
        //            if (pos != null)
        //            {
        //                item.MaBoPhan = pos.Ma;
        //            }
        //        }
        //    }

        //    return rawData;
        //}


        public List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang)
        {
            var today = DateTime.Now.Date;

            // 1. Lấy danh sách gói cá nhân (Query bên DB Customer)
            // Tạm thời gán MaBoPhan = MaPhongBan (nội bộ) để làm key mapping
            List<LichSuGoiDichVuDTO> goiDichVu = (
                from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                where t.MaKhachHang == MaKhachHang
                   && !(t.Deleted ?? false)
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

            // 2. Lấy danh sách gói gia đình (Query bên DB Customer)
            List<LichSuGoiDichVuDTO> goiDichVuGiaDinh = (
                from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                where t.MaKhachHang == MaKhachHang
                   && !(t.Deleted ?? false)
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

            // Gộp 2 list lại
            goiDichVu.AddRange(goiDichVuGiaDinh);

            // 3. XỬ LÝ MAPPING KHÁC DATABASE (Sửa lỗi 'Incorrect syntax near $')
            if (goiDichVu.Any())
            {
                // Lấy danh sách các Mã Phòng Ban (Internal ID) có trong kết quả
                var listPhongBanIds = goiDichVu.Select(x => x.MaBoPhan).Distinct().ToList();

                // --- CÁCH SỬA: KHÔNG DÙNG .Contains() TRONG LINQ SQL ---

                // B1: Lấy tất cả các điểm bán hàng có liên kết với phòng ban về RAM.
                // (Bảng DiemBanHang là bảng danh mục, dữ liệu ít nên query này rất nhẹ)
                var allPosLinks = _posContext.TblDiemBanHang.AsNoTracking()
                    .Where(x => x.MaPhongBan != null) // Chỉ lấy những dòng có map phòng ban
                    .Select(x => new { x.Ma, x.MaPhongBan })
                    .ToList(); // <--- Executed SQL ngay tại đây (SELECT đơn giản, không lỗi)

                // B2: Thực hiện lọc (Filter) trên RAM (Client-side evaluation)
                // Lúc này C# tự xử lý listPhongBanIds, không liên quan đến SQL Server nữa
                var mapDiemBanHang = allPosLinks
                    .Where(x => listPhongBanIds.Contains(x.MaPhongBan!.Value))
                    .ToList();

                // Duyệt qua list kết quả và tráo đổi ID
                foreach (var item in goiDichVu)
                {
                    // Tìm xem MaPhongBan hiện tại map với Điểm Bán Hàng nào
                    var pos = mapDiemBanHang.FirstOrDefault(p => p.MaPhongBan == item.MaBoPhan);

                    if (pos != null)
                    {
                        // Thay thế MaPhongBan nội bộ bằng Mã Điểm Bán Hàng
                        item.MaBoPhan = pos.Ma;
                    }
                }
            }

            return goiDichVu;
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVuByBoPhan(Guid MaKhachHang, Guid MaBoPhan)
        {
            // Lưu ý: MaBoPhan tham số truyền vào chính là MaDiemBanHang (Client gửi)

            // 1. Tìm "MaPhongBan" (Internal ID) tương ứng trong DB POS
            var maPhongBanInternal = _posContext.TblDiemBanHang.AsNoTracking()
                .Where(x => x.Ma == MaBoPhan)
                .Select(x => x.MaPhongBan)
                .FirstOrDefault();

            // Nếu không tìm thấy mapping nào -> Tức là Điểm bán hàng không tồn tại hoặc chưa cấu hình Phòng Ban
            // -> Trả về rỗng luôn cho nhanh
            if (maPhongBanInternal == null)
            {
                return new List<LichSuGoiDichVuDTO>();
            }

            // 2. Query dữ liệu bên DB Customer bằng Internal ID vừa tìm được
            var query = from ls in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                        join g in _context.TblHangHoa.AsNoTracking() on ls.MaHangHoa equals g.Ma
                        where ls.MaKhachHang == MaKhachHang
                           // So sánh với MaPhongBan nội bộ
                           && ls.MaPhongBan == maPhongBanInternal
                           && !(ls.Deleted ?? false)
                           && ls.NgayKichHoat != null
                           && ((ls.SoLanSuDung ?? 0) - (ls.SoLanDaSuDung ?? 0) > 0)
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

                            // Trả về lại đúng cái Mã mà client đã gửi xuống (MaDiemBanHang)
                            // Không cần lấy từ DB nữa vì ta đang filter theo chính nó
                            MaBoPhan = MaBoPhan
                        };

            return query.ToList();
        }
    }
}

