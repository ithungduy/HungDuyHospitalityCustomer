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

        //// dùng cho check lịch sử đăng kí
        public List<LichSuGoiDichVuDTO> GetListGoiDichVu(Guid MaKhachHang)
        {
            // BƯỚC 1: Lấy dữ liệu thô (Raw Data)
            // Code này chạy SQL và lấy dữ liệu về RAM
            var rawDataTemp = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                               join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                               where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                               select new
                               {
                                   MaLichSuGoiDichVu = t.Ma,
                                   TenGoiDichVu = dv.Ten,
                                   NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
                                   NgayHetHan = t.NgayHetHan,
                                   SoLan = t.SoLanSuDung ?? 0,
                                   SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                   ConLai = t.SoLanConLai ?? 0,
                                   MaPhongBanRaw = t.MaPhongBan // Đây là Guid?
                               }).ToList();

            // Map sang DTO
            var listGoiDichVu = rawDataTemp.Select(x => new LichSuGoiDichVuDTO
            {
                MaLichSuGoiDichVu = x.MaLichSuGoiDichVu,
                TenGoiDichVu = x.TenGoiDichVu,
                NgayKichHoat = x.NgayKichHoat,
                NgayHetHan = x.NgayHetHan,
                SoLan = x.SoLan,
                SoLanDaSuDung = x.SoLanDaSuDung,
                ConLai = x.ConLai,

                // --- ĐÃ SỬA Ở ĐÂY ---
                // Vì x.MaPhongBanRaw đã là Guid? nên dùng trực tiếp, 
                // không cần Utility.GetGuid (hàm này chỉ dành cho string)
                MaBoPhan = x.MaPhongBanRaw ?? Utility.defaultUID,

                MaDiemBanHang = Guid.Empty
            }).ToList();


            // BƯỚC 2: Lấy dữ liệu gói gia đình (Tương tự)
            var rawFamilyTemp = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                                 join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                                 join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                                 where t.MaKhachHang == MaKhachHang && !(t.Deleted ?? false)
                                 select new
                                 {
                                     MaLichSuGoiDichVu = gdv.Ma,
                                     TenGoiDichVu = dv.Ten,
                                     NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
                                     NgayHetHan = gdv.NgayHetHan,
                                     SoLan = gdv.SoLanSuDung ?? 0,
                                     SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
                                     ConLai = gdv.SoLanConLai ?? 0,
                                     MaPhongBanRaw = gdv.MaPhongBan
                                 }).ToList();

            var listGiaDinh = rawFamilyTemp.Select(x => new LichSuGoiDichVuDTO
            {
                MaLichSuGoiDichVu = x.MaLichSuGoiDichVu,
                TenGoiDichVu = x.TenGoiDichVu,
                NgayKichHoat = x.NgayKichHoat,
                NgayHetHan = x.NgayHetHan,
                SoLan = x.SoLan,
                SoLanDaSuDung = x.SoLanDaSuDung,
                ConLai = x.ConLai,

                // --- ĐÃ SỬA Ở ĐÂY ---
                MaBoPhan = x.MaPhongBanRaw ?? Utility.defaultUID,

                MaDiemBanHang = Guid.Empty
            }).ToList();

            // Gộp danh sách
            listGoiDichVu.AddRange(listGiaDinh);

            // BƯỚC 3: Xử lý Mapping (Giữ nguyên)
            if (listGoiDichVu.Any())
            {
                MapDiemBanHangFromPos(listGoiDichVu);
            }

            return listGoiDichVu;
        }


        // Tôi cũng update hàm này luôn để dữ liệu đồng bộ (tránh việc hàm trên có MaDiemBanHang mà hàm này lại không)
        public List<LichSuGoiDichVuDTO> GetListGoiDichVuConSuDung(Guid MaKhachHang)
        {
            var today = DateTime.Now.Date;
            var now = DateTime.Now;

            // =========================================================================
            // 1. Lấy danh sách gói cá nhân
            // =========================================================================

            // Bước 1.1: Query dữ liệu thô về RAM (tránh lỗi InvalidCast trong SQL)
            var rawDataTemp = (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                               join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                               where t.MaKhachHang == MaKhachHang
                                  && !(t.Deleted ?? false)
                                  && (t.SoLanSuDung ?? 0) - (t.SoLanDaSuDung ?? 0) > 0
                                  && (t.NgayKichHoat == null || t.NgayKichHoat <= now)
                                  && (t.NgayHetHan != null && t.NgayHetHan >= today)
                               select new
                               {
                                   MaLichSuGoiDichVu = t.Ma,
                                   TenGoiDichVu = dv.Ten,
                                   NgayKichHoat = t.NgayKichHoat ?? t.CreatedDate,
                                   NgayHetHan = t.NgayHetHan,
                                   SoLan = t.SoLanSuDung ?? 0,
                                   SoLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                   ConLai = t.SoLanConLai ?? 0,
                                   MaPhongBanRaw = t.MaPhongBan // Lấy nguyên giá trị (Guid?) về
                               }).ToList();

            // Bước 1.2: Map sang DTO và xử lý Null check trên RAM
            List<LichSuGoiDichVuDTO> goiDichVu = rawDataTemp.Select(x => new LichSuGoiDichVuDTO
            {
                MaLichSuGoiDichVu = x.MaLichSuGoiDichVu,
                TenGoiDichVu = x.TenGoiDichVu,
                NgayKichHoat = x.NgayKichHoat,
                NgayHetHan = x.NgayHetHan,
                SoLan = x.SoLan,
                SoLanDaSuDung = x.SoLanDaSuDung,
                ConLai = x.ConLai,
                // C# xử lý tốt việc này trên RAM
                MaBoPhan = x.MaPhongBanRaw ?? Utility.defaultUID,
                MaDiemBanHang = Guid.Empty
            }).ToList();


            // =========================================================================
            // 2. Lấy danh sách gói gia đình
            // =========================================================================

            // Bước 2.1: Query dữ liệu thô
            var rawFamilyTemp = (from t in _context.OpsGoiDichVuGiaDinh.AsNoTracking()
                                 join gdv in _context.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals gdv.Ma
                                 join dv in _context.TblHangHoa.AsNoTracking() on gdv.MaHangHoa equals dv.Ma
                                 where t.MaKhachHang == MaKhachHang
                                    && !(t.Deleted ?? false)
                                    && (gdv.SoLanSuDung ?? 0) - (gdv.SoLanDaSuDung ?? 0) > 0
                                    && (gdv.NgayKichHoat == null || gdv.NgayKichHoat <= today)
                                    && (gdv.NgayHetHan != null && gdv.NgayHetHan >= today)
                                 select new
                                 {
                                     MaLichSuGoiDichVu = gdv.Ma,
                                     TenGoiDichVu = dv.Ten,
                                     NgayKichHoat = gdv.NgayKichHoat ?? t.CreatedDate,
                                     NgayHetHan = gdv.NgayHetHan,
                                     SoLan = gdv.SoLanSuDung ?? 0,
                                     SoLanDaSuDung = gdv.SoLanDaSuDung ?? 0,
                                     ConLai = gdv.SoLanConLai ?? 0,
                                     MaPhongBanRaw = gdv.MaPhongBan // Lấy nguyên giá trị
                                 }).ToList();

            // Bước 2.2: Map sang DTO
            List<LichSuGoiDichVuDTO> goiDichVuGiaDinh = rawFamilyTemp.Select(x => new LichSuGoiDichVuDTO
            {
                MaLichSuGoiDichVu = x.MaLichSuGoiDichVu,
                TenGoiDichVu = x.TenGoiDichVu,
                NgayKichHoat = x.NgayKichHoat,
                NgayHetHan = x.NgayHetHan,
                SoLan = x.SoLan,
                SoLanDaSuDung = x.SoLanDaSuDung,
                ConLai = x.ConLai,
                MaBoPhan = x.MaPhongBanRaw ?? Utility.defaultUID,
                MaDiemBanHang = Guid.Empty
            }).ToList();

            // =========================================================================
            // 3. Gộp và Map POS
            // =========================================================================

            goiDichVu.AddRange(goiDichVuGiaDinh);

            if (goiDichVu.Any())
            {
                MapDiemBanHangFromPos(goiDichVu);
            }

            return goiDichVu;
        }

        public List<LichSuGoiDichVuDTO> GetListGoiDichVuByBoPhan(Guid MaKhachHang, Guid MaBoPhan)
        {
            // LOGIC MỚI: MaBoPhan ở đây chính là [MaPhongBan] (Internal ID)

            // BƯỚC 1: Tìm thông tin Điểm Bán Hàng tương ứng với Bộ Phận này (Để trả về cho Client dùng)
            // Một bộ phận có thể có nhiều điểm bán, ta lấy cái đầu tiên tìm thấy làm đại diện.
            var posInfo = _posContext.TblDiemBanHang.AsNoTracking()
                .Where(x => x.MaPhongBan == MaBoPhan)
                .Select(x => x.Ma) // Chỉ cần lấy Guid Ma (POS ID)
                .FirstOrDefault();

            // Nếu không tìm thấy POS nào map với bộ phận này, gán mặc định rỗng
            Guid posId = posInfo != null ? posInfo : Guid.Empty;

            // BƯỚC 2: Query dữ liệu lịch sử (Dùng trực tiếp MaBoPhan để tìm)

            // Lưu ý: Vẫn cần xử lý vụ Guid/String nếu DB Customer lưu MaPhongBan là String
            // Nếu DB Customer lưu MaPhongBan là Guid chuẩn rồi thì bỏ đoạn convert này đi
            // string internalIdStr = MaBoPhan.ToString(); 

            // Query dữ liệu thô về RAM
            var rawDataTemp = (from ls in _context.OpsLichSuMuaGoiDichVu.AsNoTracking()
                               join g in _context.TblHangHoa.AsNoTracking() on ls.MaHangHoa equals g.Ma
                               where ls.MaKhachHang == MaKhachHang
                                  // LOGIC CHUẨN: So sánh trực tiếp với MaPhongBan trong bảng
                                  // (Giả sử trong DB nó là Guid? như anh mô tả "uniqueidentifier Checked")
                                  && ls.MaPhongBan == MaBoPhan

                                  && !(ls.Deleted ?? false)
                                  && ls.NgayKichHoat != null
                                  && ((ls.SoLanSuDung ?? 0) - (ls.SoLanDaSuDung ?? 0) > 0)
                                  && (ls.NgayHetHan == null || ls.NgayHetHan.Value.Date >= DateTime.Now.Date)
                               orderby ls.NgayHetHan ascending
                               select new
                               {
                                   MaLichSuGoiDichVu = ls.Ma,
                                   TenGoiDichVu = g.Ten,
                                   NgayKichHoat = ls.NgayKichHoat,
                                   NgayHetHan = ls.NgayHetHan,
                                   SoLan = ls.SoLanSuDung ?? 0,
                                   SoLanDaSuDung = ls.SoLanDaSuDung ?? 0,
                                   ConLai = ls.SoLanConLai ?? 0,
                                   MaPhongBanRaw = ls.MaPhongBan
                               }).ToList();

            // BƯỚC 3: Map sang DTO
            var result = rawDataTemp.Select(x => new LichSuGoiDichVuDTO
            {
                MaLichSuGoiDichVu = x.MaLichSuGoiDichVu,
                TenGoiDichVu = x.TenGoiDichVu,
                NgayKichHoat = x.NgayKichHoat,
                NgayHetHan = x.NgayHetHan,
                SoLan = x.SoLan,
                SoLanDaSuDung = x.SoLanDaSuDung,
                ConLai = x.ConLai,

                // 1. MaBoPhan: Chính là tham số đầu vào (Internal ID)
                MaBoPhan = MaBoPhan,

                // 2. MaDiemBanHang: Lấy từ kết quả query bảng TblDiemBanHang ở Bước 1
                MaDiemBanHang = posId
            }).ToList();

            return result;
        }

        // Hàm Private Helper để tái sử dụng logic Mapping (Clean Code)
        private void MapDiemBanHangFromPos(List<LichSuGoiDichVuDTO> dataList)
        {
            // 1. Lấy danh sách ID phòng ban nội bộ cần map
            var listPhongBanIds = dataList.Select(x => x.MaBoPhan).Distinct().ToList();

            // 2. Lấy bảng map từ DB POS (Chỉ lấy Ma và MaPhongBan)
            // Lưu ý: DB POS cũng phải chắc chắn MaPhongBan là Guid, nếu POS là String thì báo mình sửa đoạn này nhé.
            // (Giả sử bên POS chuẩn là Guid rồi)
            var allPosLinks = _posContext.TblDiemBanHang.AsNoTracking()
                .Where(x => x.MaPhongBan != null)
                .Select(x => new { x.Ma, x.MaPhongBan })
                .ToList();

            // 3. Filter trên RAM
            var mapDiemBanHang = allPosLinks
                .Where(x => listPhongBanIds.Contains(x.MaPhongBan!.Value))
                .ToList();

            // 4. Cập nhật MaDiemBanHang vào List kết quả
            foreach (var item in dataList)
            {
                var pos = mapDiemBanHang.FirstOrDefault(p => p.MaPhongBan == item.MaBoPhan);
                if (pos != null)
                {
                    item.MaDiemBanHang = pos.Ma;
                }
            }
        }
    }
}

