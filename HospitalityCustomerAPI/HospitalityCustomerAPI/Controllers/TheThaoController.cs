using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class TheThaoController : ApiControllerBase
    {
        private readonly HungDuyHospitalityCustomerContext _customerContext;
        public TheThaoController(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext) : base(hungDuyHospitalityCustomerContext)
        {
            _customerContext = hungDuyHospitalityCustomerContext;
        }

        [HttpGet("getLichTapLuyen")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> getLichTapLuyen(string Thang, string Nam, string MaKhachHang)
        {
            int thang = Thang.ToInt();
            int nam = Nam.ToInt();
            Guid maKhachHang = MaKhachHang.GetGuid();

            // FIX 1: Đưa validate lên đầu tiên để tránh crash hàm DaysInMonth
            if (thang < 1 || thang > 12) return new ResponseModelError("Tháng không hợp lệ");
            if (nam < 1) return new ResponseModelError("Năm không hợp lệ");

            var list = new List<LichNgay>();
            var culture = new CultureInfo("vi-VN");

            int soNgay = DateTime.DaysInMonth(nam, thang);
            DateTime dtpTuNgay = new DateTime(nam, thang, 1);
            DateTime dtpDenNgay = new DateTime(nam, thang, soNgay);

            for (int i = 1; i <= soNgay; i++)
            {
                var ngay = new DateTime(nam, thang, i);
                list.Add(new LichNgay
                {
                    Ngay = ngay,
                    Thu = culture.DateTimeFormat.GetDayName(ngay.DayOfWeek),
                    NgayTrongThang = i,
                    Thang = thang,
                    Nam = nam,
                    CoLichTap = false
                });
            }

            var ngayTapTrongThang = await _customerContext.SchLichTapLuyen
                .AsNoTracking()
                .Where(t => t.NgayTapLuyen.HasValue
                            && t.NgayTapLuyen.Value.Date >= dtpTuNgay.Date
                            && t.NgayTapLuyen.Value.Date <= dtpDenNgay.Date
                            && !(t.Deleted ?? false))
                .Select(t => t.NgayTapLuyen!.Value.Date)
                .Distinct()
                .ToListAsync();

            var ngayDangKyTrongThang = await (from t in _customerContext.SchDangKyTap.AsNoTracking()
                                                       .Where(t => t.MaKhachHang == maKhachHang && !(t.Deleted ?? false))
                                              join _l in _customerContext.SchLichTapLuyen.AsNoTracking() on t.MaLichTapLuyen equals _l.Ma into _l
                                              from l in _l.DefaultIfEmpty()
                                              where !(l.Deleted ?? false)
                                              select new
                                              {
                                                  NgayTapLuyen = l != null ? l.NgayTapLuyen : t.NgayTapLuyen,
                                              }).Distinct().ToListAsync();

            var setNgayTap = new HashSet<DateTime>(ngayTapTrongThang);
            var setNgayDangKy = new HashSet<DateTime>(
                ngayDangKyTrongThang
                    .Where(x => x.NgayTapLuyen.HasValue)
                    .Select(x => x.NgayTapLuyen.Value.Date)
            );

            foreach (var d in list)
            {
                if (setNgayTap.Contains(d.Ngay.Date))
                    d.CoLichTap = true;

                if (setNgayDangKy.Contains(d.Ngay.Date))
                    d.CoDangKy = true;
            }

            return new ResponseModelSuccess("", list);
        }

        [HttpGet("getGioTapTheoNgay")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> getGioTapTheoNgay(string Ngay, string MaKhachHang)
        {
            Guid maKhachHang = MaKhachHang.GetGuid();
            DateTime dtpNgay = Ngay.ToDateTime2(DateTime.Now).Value.Date;

            // 1. Tạo Query gốc (Chưa execute) để tái sử dụng
            var queryLichTapTrongNgay = _customerContext.SchLichTapLuyen.AsNoTracking()
                .Where(t => t.NgayTapLuyen.HasValue
                            && t.NgayTapLuyen.Value.Date == dtpNgay
                            && !(t.Deleted ?? false));

            // 2. Lấy danh sách thông tin cơ bản của Lớp học từ Query trên
            var listDataTemp = await queryLichTapTrongNgay
                .Select(x => new
                {
                    ma = x.Ma,
                    maHuanLuyenVien = x.MaHuanLuyenVien,
                    tenHuanLuyenVien = x.TenHuanLuyenVien,
                    tenPhongTap = x.TenPhongTap,
                    tuGio = x.TuGio,
                    denGio = x.DenGio,
                    noiDung = x.NoiDung,
                    soHocVien = x.SoHocVien,                  
                    ngayTapLuyen = x.NgayTapLuyen
                }).ToListAsync();

            // 3. Lấy số lượng đã đăng ký (Giữ nguyên)
            var dictSoDangKy = await (from dk in _customerContext.SchDangKyTap.AsNoTracking()
                                      join lt in queryLichTapTrongNgay on dk.MaLichTapLuyen equals lt.Ma
                                      where !(dk.Deleted ?? false)
                                      group dk by dk.MaLichTapLuyen into g
                                      select new { MaLichTapLuyen = g.Key, So = g.Count() })
                                      .ToDictionaryAsync(k => k.MaLichTapLuyen!.Value, v => v.So);

            // 4. Lấy danh sách đăng ký CỦA KHÁCH HÀNG (Giữ nguyên)
            var listDangKyTheoKhachHang = await (from dk in _customerContext.SchDangKyTap.AsNoTracking()
                                                 join lt in queryLichTapTrongNgay on dk.MaLichTapLuyen equals lt.Ma
                                                 where dk.MaKhachHang == maKhachHang && !(dk.Deleted ?? false)
                                                 select new
                                                 {
                                                     maDangKy = dk.Ma,
                                                     maLichTap = dk.MaLichTapLuyen,
                                                     ngayDangKy = dk.CreatedDate,
                                                 }).ToListAsync();

            // 5. Ghép dữ liệu lại để trả về Client
            var listData = listDataTemp
                .Select(x =>
                {
                    dictSoDangKy.TryGetValue(x.ma, out var soDangKy);
                    return new
                    {
                        x.ma,
                        x.maHuanLuyenVien,
                        x.tenHuanLuyenVien,
                        x.tenPhongTap,
                        x.tuGio,
                        x.denGio,
                        x.noiDung,
                        x.soHocVien,                      
                        x.ngayTapLuyen,
                        soHocVienDangKy = soDangKy,
                        choTrong = Math.Max(0, (x.soHocVien ?? 0) - soDangKy),
                        DangKys = listDangKyTheoKhachHang
                                     .Where(a => a.maLichTap == x.ma)
                                     .ToList(),
                    };
                })
                .OrderBy(x => x.tuGio)
                .ToList();

            return new ResponseModelSuccess("", listData);
        }

        [HttpPost("dangKyLichTap")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> setLichTap(string MaKhachHang, string MaLichSuGoiDichVu, string MaLichTap)
        {
            Guid maKhachHang = MaKhachHang.GetGuid();
            Guid maLichSuGoiDichVu = MaLichSuGoiDichVu.GetGuid();
            Guid maLichTap = MaLichTap.GetGuid();

            using var transaction = _customerContext.Database.BeginTransaction();
            try
            {
                // FIX 3: Bỏ AsNoTracking để có thể Update số lần còn lại
                var goiDichVu = await _customerContext.OpsLichSuMuaGoiDichVu
                                    .FirstOrDefaultAsync(x => x.Ma == maLichSuGoiDichVu && !(x.Deleted ?? false));

                if (goiDichVu == null) return new ResponseModelError("Gói dịch vụ không tồn tại");

                var now = DateTime.Now;
                if (goiDichVu.NgayHetHan.HasValue && goiDichVu.NgayHetHan.Value.Date < now.Date)
                    return new ResponseModelError("Gói dịch vụ quá hạn sử dụng");

                if ((goiDichVu.SoLanConLai ?? 0) <= 0)
                    return new ResponseModelError("Gói dịch vụ hết số lần sử dụng");

                // Check trùng
                var kiemTraTrung = await _customerContext.SchDangKyTap.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang
                                                    && x.MaGoiDichVu == maLichSuGoiDichVu
                                                    && x.MaLichTapLuyen == maLichTap
                                                    && !(x.Deleted ?? false));

                // Check Full Slot
                var lopHoc = await _customerContext.SchLichTapLuyen.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == maLichTap);
                if (lopHoc == null) return new ResponseModelError("Lớp học không tồn tại");

                var soLuongDaDangKy = await _customerContext.SchDangKyTap
                                    .CountAsync(x => x.MaLichTapLuyen == maLichTap && !(x.Deleted ?? false));

                if (soLuongDaDangKy >= (lopHoc.SoHocVien ?? 0))
                    return new ResponseModelError("Lớp học đã đầy, vui lòng chọn giờ khác");

                if (!lopHoc.NgayTapLuyen.HasValue || !lopHoc.TuGio.HasValue)
                    return new ResponseModelError("Thiếu ngày/giờ tập luyện");

                DateTime ngayGioTap = lopHoc.NgayTapLuyen.Value.Date
                                      .Add(lopHoc.TuGio.Value.ToTimeSpan());

                // Không cho đăng ký nếu hiện tại đã nằm trong vòng 10 phút trước giờ tập
                if (now > ngayGioTap.AddMinutes(-10))
                {
                    return new ResponseModelError("Phải đăng ký trước 10 phút");
                }

                if (kiemTraTrung != null) return new ResponseModelError("Lịch tập đã đăng ký rồi");

                // Tạo đăng ký mới
                SchDangKyTap item = new SchDangKyTap
                {
                    MaKhachHang = maKhachHang,
                    MaGoiDichVu = maLichSuGoiDichVu,
                    MaLichTapLuyen = maLichTap,
                    CreatedDate = now,
                    UserCreated = objToken.userid,
                };
                await _customerContext.AddAsync(item);
               
                goiDichVu.SoLanDaSuDung = (goiDichVu.SoLanDaSuDung ?? 0) + 1;
                goiDichVu.SoLanConLai = (goiDichVu.SoLanSuDung ?? 0) - (goiDichVu.SoLanDaSuDung ?? 0);
                
                _customerContext.Update(goiDichVu);
                await _customerContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ResponseModelSuccess("Đã đăng ký thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseModelError(ex.Message);
            }
        }

        [HttpPost("huyLichTap")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> delLichTap(string Ma)
        {
            Guid ma = Ma.GetGuid();

            using var transaction = _customerContext.Database.BeginTransaction();
            try
            {
                var now = DateTime.Now;

                // 1. Lấy thông tin đăng ký KÈM THEO thông tin lớp học
                var item = await (from dk in _customerContext.SchDangKyTap
                                  join lt in _customerContext.SchLichTapLuyen on dk.MaLichTapLuyen equals lt.Ma
                                  where dk.Ma == ma && !(dk.Deleted ?? false)
                                  select new { DangKy = dk, LichTap = lt }).FirstOrDefaultAsync();

                if (item == null) return new ResponseModelError("Lịch đăng ký không tồn tại");

                // 2. CHECK RULE 1: Không cho hủy nếu đã đăng ký quá 24h
                if (item.DangKy.CreatedDate.HasValue && item.DangKy.CreatedDate.Value.AddHours(24) < now)
                {
                    return new ResponseModelError("Lịch đã đăng ký quá 24h, không thể hủy.");
                }

                // 3. CHECK RULE 2: Validate thời gian
                if (item.LichTap.NgayTapLuyen.HasValue && item.LichTap.TuGio.HasValue)
                {
                    // Tính thời gian bắt đầu chính xác
                    DateTime thoiGianBatDau = item.LichTap.NgayTapLuyen.Value.Date
                                              .Add(item.LichTap.TuGio.Value.ToTimeSpan());

                    // CASE A: Nếu lớp học ĐÃ diễn ra rồi (Quá khứ) -> Chặn luôn
                    if (now >= thoiGianBatDau)
                    {
                        return new ResponseModelError("Lớp học đã bắt đầu hoặc kết thúc, không thể hủy.");
                    }

                    // CASE B: Nếu lớp học CHƯA diễn ra (Tương lai), nhưng còn ít hơn 2 tiếng
                    // Logic: Nếu (Bây giờ + 2 tiếng) mà vượt quá (lớn hơn) giờ bắt đầu -> Tức là khoảng cách < 2h
                    if (now.AddHours(2) > thoiGianBatDau)
                    {
                        return new ResponseModelError("Không thể hủy lịch trước giờ tập 2 tiếng.");
                    }
                }

                // 4. Xóa lịch đăng ký (Soft delete)
                item.DangKy.Deleted = true;
                item.DangKy.DeletedDate = now;
                item.DangKy.UserDeleted = objToken.userid;

                _customerContext.Update(item.DangKy);

                // =================================================================
                // 5. LOGIC HOÀN LẠI LƯỢT TẬP (REFUND)
                // =================================================================
                if (item.DangKy.MaGoiDichVu.HasValue)
                {
                    // Tìm gói dịch vụ mà khách đã dùng để book
                    var goiDichVu = await _customerContext.OpsLichSuMuaGoiDichVu
                                        .FirstOrDefaultAsync(x => x.Ma == item.DangKy.MaGoiDichVu);

                    if (goiDichVu != null)
                    {
                        
                        goiDichVu.SoLanDaSuDung = (goiDichVu.SoLanDaSuDung ?? 0) - 1;

                        // Safety check: Không để số lần đã dùng âm
                        if (goiDichVu.SoLanDaSuDung < 0) goiDichVu.SoLanDaSuDung = 0;

                        // Tính lại số lần còn lại dựa trên tổng và đã dùng
                        goiDichVu.SoLanConLai = (goiDichVu.SoLanSuDung ?? 0) - (goiDichVu.SoLanDaSuDung ?? 0);
                     

                        _customerContext.Update(goiDichVu);
                    }
                }
                // =================================================================

                await _customerContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModelSuccess("Đã huỷ và hoàn lại lượt tập thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseModelError(ex.Message);
            }
        }
    }

    public class LichNgay

    {

        public DateTime Ngay { get; set; }

        public string Thu { get; set; }

        public int NgayTrongThang { get; set; }

        public int Thang { get; set; }

        public int Nam { get; set; }

        public bool CoLichTap { get; set; } = false;

        public bool CoDangKy { get; set; } = false;

    }
}