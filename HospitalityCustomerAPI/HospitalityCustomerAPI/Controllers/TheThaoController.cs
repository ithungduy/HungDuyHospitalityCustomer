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
                    Thu = culture.DateTimeFormat.GetDayName(ngay.DayOfWeek), // "thứ hai", "chủ nhật", ...
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
                                              join l in _customerContext.SchLichTapLuyen.AsNoTracking() on t.MaLichTapLuyen equals l.Ma
                                              where !(l.Deleted ?? false)
                                              select new
                                              {
                                                  l.NgayTapLuyen
                                              }).Distinct().ToListAsync();

            var setNgayTap = new HashSet<DateTime>(ngayTapTrongThang);
            var setngayDangKyTrongThang = new HashSet<DateTime>(ngayTapTrongThang);

            foreach (var d in list)
            {
                if (setNgayTap.Contains(d.Ngay.Date))
                    d.CoLichTap = true;

                if (setngayDangKyTrongThang.Contains(d.Ngay.Date))
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

            var listDataTemp = await _customerContext.SchLichTapLuyen
                .AsNoTracking()
                .Where(t => t.NgayTapLuyen.HasValue
                            && t.NgayTapLuyen.Value.Date == dtpNgay
                            && !(t.Deleted ?? false))
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
                }).ToListAsync();

            var maLichTapIds = listDataTemp.Select(x => x.ma).ToList();

            var listDangKy = _customerContext.SchDangKyTap
               .AsNoTracking()
               .Where(d => d.MaLichTapLuyen.HasValue
                           && maLichTapIds.Contains(d.MaLichTapLuyen.Value)
                           && !(d.Deleted ?? false));

            var dictSoDangKy = await listDangKy
                .GroupBy(d => d.MaLichTapLuyen!.Value)
                .Select(g => new { MaLichTapLuyen = g.Key, So = g.Count() })
                .ToDictionaryAsync(k => k.MaLichTapLuyen, v => v.So);

            var listDangKyTheoKhachHang = await listDangKy.Where(x => x.MaKhachHang == maKhachHang).Select(x=> new
            {
                maLichTap = x.MaLichTapLuyen,
                ngayDangKy = x.CreatedDate,
            }).ToListAsync();

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
                    soHocVienDangKy = soDangKy,
                    choTrong = Math.Max(0, (x.soHocVien ?? 0) - soDangKy),
                    DangKys = listDangKyTheoKhachHang.Where(a => a.maLichTap == x.ma).ToList(),
                };
            }).OrderBy(x => x.tuGio).ToList();
            return new ResponseModelSuccess("", listData);
        }


        [HttpGet("getGioTapTheoNgay")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> setLichTap(string MaKhachHang, string MaLichSuGoiDichVu, string MaLichTap)
        {
            Guid maKhachHang = MaKhachHang.GetGuid();
            Guid maLichSuGoiDichVu = MaLichSuGoiDichVu.GetGuid();
            Guid maLichTap = MaLichTap.GetGuid();

            // kiểm tra gói hết hạn chưa, số lần còn ko
            // kiểm tra khách đăng ký trùng
            var goiDichVu = await _customerContext.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == maLichSuGoiDichVu && !(x.Deleted ?? false));
            if (goiDichVu == null)
            {
                return new ResponseModelError("Gói dịch vụ không tồn tại");
            }
            var now = DateTime.Now;
            if (goiDichVu.NgayHetHan.HasValue && goiDichVu.NgayHetHan.Value.Date < now.Date)
            {
                return new ResponseModelError("Gói dịch vụ quá hạn sử dụng");
            }
            if ((goiDichVu.SoLanConLai ?? 0) <= 0)
            {
                return new ResponseModelError("Gói dịch vụ hết số lần sử dụng");
            }
            var kiemTraTrung = await _customerContext.SchDangKyTap.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang 
                                                && x.MaGoiDichVu == maLichSuGoiDichVu 
                                                && x.MaLichTapLuyen == maLichTap 
                                                && !(x.Deleted ?? false));

            if (kiemTraTrung != null)
            {
                return new ResponseModelError("Lịch tập đã đăng ký rồi");
            }

            SchDangKyTap item = new SchDangKyTap
            {
                MaKhachHang = maKhachHang,
                MaGoiDichVu = maLichSuGoiDichVu,
                MaLichTapLuyen = maLichTap,
                CreatedDate = now,
                UserCreated = objToken.userid,
            };
            try
            {
                await _customerContext.AddAsync(item);
                await _customerContext.SaveChangesAsync();
                return new ResponseModelSuccess("Đã đăng ký thành công");
            }
            catch (Exception ex)
            {
                return new ResponseModelError(ex.Message);
            }
        }

        [HttpGet("getGioTapTheoNgay")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> delLichTap(string Ma)
        {
            Guid ma = Ma.GetGuid();

            var now = DateTime.Now;
            var item = await _customerContext.SchDangKyTap.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == ma && !(x.Deleted ?? false));

            if (item == null)
            {
                return new ResponseModelError("Lịch đăng ký không tồn tại");
            }

            if (item.CreatedDate.HasValue && item.CreatedDate.Value.AddHours(24) < now)
            {
                return new ResponseModelError("Lịch đăng ký quá 24 giờ không thể huỷ");
            }         

            try
            {
                item.Deleted = true;
                item.DeletedDate = now;
                item.UserDeleted = objToken.userid;

                _customerContext.Update(item);
                await _customerContext.SaveChangesAsync();
                return new ResponseModelSuccess("Đã huỷ thành công");
            }
            catch (Exception ex)
            {
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
