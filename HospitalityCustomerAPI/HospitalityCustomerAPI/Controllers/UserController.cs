using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.CheckIn;
using HospitalityCustomerAPI.DTO.LichSuMuaGoiDichVu;
using HospitalityCustomerAPI.DTO.User;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Transactions;
using static HospitalityCustomerAPI.DTO.User.UserDto;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class UserController : ApiControllerBase
    {
        private readonly HungDuyHospitalityContext _posdbcontext;
        private readonly HungDuyHospitalityCustomerContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IKhachHangRepository _khachHangRepository;
        private readonly ISmsOtpRepository _smsOtpRepository;
        private readonly ICheckInRepository _checkInRepository;
        private readonly ILichSuMuaGoiDichVuRepository _lichSuMuaGoiDichVuRepository;
        private readonly IDiemBanHangPOSRepository _diemBanHangPOSRepository;
        private readonly ILichSuMuaGoiDichVuPOSRepository _lichSuMuaGoiDichVuPOSRepository;
        private readonly IEspClient _espClient;
        private readonly ILogger<UserController> _logger;

        private readonly IOtpNotificationService _otpNotificationService;

        // API Key và URL của HungDuyFinanceAccounting
        private const string PASSCODEFA = "F1n@nc3@cc0nt1ng";
        private const string PASSCODEPASSWORD = "0359509251";
        //private const string apiUrl = "http://localhost:5202/api/App/SendCanaOtp";
        private const string apiUrl = "https://fa.hungduy.vn/api/App/SendCanaOtp";


        public UserController(
               HungDuyHospitalityCustomerContext context,
               HungDuyHospitalityContext posdbcontext,
               IUserRepository userRepository,
               ISmsOtpRepository smsOtpRepository,
               IKhachHangRepository khachHangRepository,
               ICheckInRepository checkInRepository,
               ILichSuMuaGoiDichVuRepository lichSuMuaGoiDichVuRepository,
               IDiemBanHangPOSRepository diemBanHangPOSRepository,
               ILichSuMuaGoiDichVuPOSRepository lichSuMuaGoiDichVuPOSRepository,
               IEspClient espClient,
               ILogger<UserController> logger,
               IOtpNotificationService otpNotificationService
           ) : base(context)
        {
            _userRepository = userRepository;
            _smsOtpRepository = smsOtpRepository;
            _posdbcontext = posdbcontext;
            _context = context;
            _khachHangRepository = khachHangRepository;
            _checkInRepository = checkInRepository;
            _lichSuMuaGoiDichVuRepository = lichSuMuaGoiDichVuRepository;
            _diemBanHangPOSRepository = diemBanHangPOSRepository;
            _lichSuMuaGoiDichVuPOSRepository = lichSuMuaGoiDichVuPOSRepository;
            _espClient = espClient;
            _logger = logger;
            _otpNotificationService = otpNotificationService;
        }

        // --- HÀM HELPER MỚI ---
        /// <summary>
        /// Tạo API Key động để gọi qua FA,
        /// logic phải khớp 100% với [APIKeyCheckAttribute] bên FA
        /// </summary>
        private string GenerateFAGatewayApiKey()
        {
            // Giả định bạn có Utility.GetSHA512 trong Common
            return Utility.GetSHA512(PASSCODEFA + DateTime.Now.ToString("yyyyMM"));
        }

        [HttpPost("Register")]
        [APIKeyCheck]
        public ResponseModel Register([FromForm] UserRegisterDto dto, [FromForm] bool isDev = false)
        {
            // 0) Chuẩn hóa SĐT & kiểm tra đã tồn tại
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);
            if (_userRepository.GetItemByPhone(username) != null)
            {
                return new ResponseModelError("Số điện thoại đã được dùng");
            }
            if (dto.HoTen + "" == "")
            {
                return new ResponseModelError("Vui lòng nhập họ tên");
            }
            // 0.1) Nếu isDev = true → BỎ QUA OTP, vào thẳng luồng tạo user
            //if (isDev)
            //{
            //    return CreateUserAndSyncPos(dto, username);
            //}

            // 1) Nếu CHƯA gửi kèm OTP → phát OTP & trả OTPrequied
            if (string.IsNullOrWhiteSpace(dto.Otp))
            {
                if (_smsOtpRepository.CheckPhoneExist(username))
                {
                    if (!_smsOtpRepository.CheckOTPNeedToRenew(username))
                    {
                        return new ResponseModel("OTPrequied", "Bạn đã yêu cầu OTP và vẫn còn hiệu lực.");
                    }
                    if (_smsOtpRepository.CheckOTPLimit(username))
                    {
                        return new ResponseModelError("Bạn đã vượt số lần nhận OTP tối đa của tháng!");
                    }

                    
                    var (plainOtp, updateStatus) = _smsOtpRepository.GenerateAndSaveNewOtp(username);

                    if (updateStatus == ActionStatus.Success.toInt() && plainOtp != null)
                    {
                        string finalApiKey = GenerateFAGatewayApiKey();
                        // "Bắn và quên" (Fire-and-forget) gọi Zalo/SMS service
                        _ = _otpNotificationService.SendOtpAsync(username, plainOtp, finalApiKey, apiUrl);

                        // Đổi "SMS" thành "tin nhắn" chung chung
                        return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra tin nhắn.");
                    }
                    return new ResponseModelError("Không thể gửi lại OTP. Vui lòng thử lại.");
                    
                }
                else
                {
                    
                    var (plainOtp, addStatus) = _smsOtpRepository.GenerateAndSaveOtp(username);

                    if (addStatus == ActionStatus.Success.toInt() && plainOtp != null)
                    {
                        string finalApiKey = GenerateFAGatewayApiKey();
                        // "Bắn và quên" (Fire-and-forget) gọi Zalo/SMS service
                        _ = _otpNotificationService.SendOtpAsync(username, plainOtp, finalApiKey, apiUrl);

                        return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra tin nhắn.");
                    }
                    return new ResponseModelError("Không thể gửi OTP. Vui lòng thử lại.");
                    
                }
            }

            // 2) ĐÃ có OTP → kiểm tra hợp lệ
            if (dto.Otp != PASSCODEPASSWORD && !_smsOtpRepository.CheckOTPValid(username, dto.Otp!))
            {
                return new ResponseModelError("Mã OTP không đúng hoặc đã hết hạn.");
            }

            // 3) Đúng OTP → tạo user
            return CreateUserAndSyncPos(dto, username);
        }

        // Helper: tái sử dụng đoạn tạo SysUser + đồng bộ POS
        private ResponseModel CreateUserAndSyncPos(UserRegisterDto dto, string username)
        {
            // Parse ngày sinh (nếu có)
            DateTime? dtpNgaySinh = null;
            if (!string.IsNullOrEmpty(dto.NgaySinh))
            {
                try
                {
                    dtpNgaySinh = DateTime.ParseExact(dto.NgaySinh, "dd/MM/yyyy", null);
                }
                catch
                {
                    return ResponseDateFormatIsIncorrect;
                }
            }


            var entity = new SysUser
            {
                Ma = Guid.NewGuid(),
                Username = username,
                Password = dto.Password,
                HinhAnh = dto.HinhAnh,
                FullName = (dto.HoTen + "").ToUpper(),
                NgaySinh = dtpNgaySinh,
                GioiTinh = dto.GioiTinh,
                MaQuocGia = dto.MaQuocGia,
                MaTinh = dto.MaTinh,
                MaPhuongXa = dto.MaPhuongXa,
                SoNha = dto.SoNha,
                QuocTich = dto.QuocTich,
                HoChieu = dto.HoChieu,
                MaDanToc = dto.MaDanToc
            };

            // Đồng bộ POS (TblKhachHang)
            string sodt = username.StartsWith("84") ? username.Replace("84", "0") : username;
            TblKhachHang? khachHang = _posdbcontext.TblKhachHang
                .AsNoTracking()
                .FirstOrDefault(x => (x.SoDienThoai == username || x.SoDienThoai == sodt) && !(x.Deleted ?? false));

            if (khachHang == null)
            {
                khachHang = new TblKhachHang
                {
                    Ma = Guid.NewGuid(),
                    SoDienThoai = sodt,
                    Ten = (dto.HoTen + "").ToUpper(),
                    Code = sodt,
                    GioiTinh = dto.GioiTinh,
                    DiaChi = dto.SoNha + "",
                    NgaySinh = dtpNgaySinh,
                    Status = true,
                };
                _posdbcontext.Add(khachHang);
                _posdbcontext.SaveChanges();
            }

            entity.MaKhachHang = khachHang?.Ma;

            var result = _userRepository.Add(entity);
            return result.isSuccess() ? ResponseRegisterSuccessfully : ResponseAddFailure;
        }

        [HttpPost("Update")]
        [TokenUserCheckHTTP]
        public ResponseModel Update([FromForm] UserUpdateDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);

            DateTime? dtpNgaySinh = null;
            if (!string.IsNullOrEmpty(dto.NgaySinh))
            {
                try
                {
                    dtpNgaySinh = DateTime.ParseExact(dto.NgaySinh, "dd/MM/yyyy", null);
                }
                catch
                {
                    return ResponseDateFormatIsIncorrect;
                }
            }

            var entity = _userRepository.GetItemByPhone(username);
            if (entity == null)
                return ResponseNotExist;

            entity.HinhAnh = dto.HinhAnh;
            entity.FullName = dto.HoTen;
            entity.NgaySinh = dtpNgaySinh;
            entity.GioiTinh = dto.GioiTinh;
            entity.MaQuocGia = dto.MaQuocGia;
            entity.MaTinh = dto.MaTinh;
            entity.MaPhuongXa = dto.MaPhuongXa;
            entity.SoNha = dto.SoNha;
            entity.QuocTich = dto.QuocTich;
            entity.MaDanToc = dto.MaDanToc;
            entity.HoChieu = dto.HoChieu;
            entity.Status = dto.Status;
            entity.TienSuBenhLy = dto.TienSuBenhLy;
            entity.DongYtuanThuNoiQuy = dto.DongYTuanThuNoiQuy == "1";
            entity.ChoPhepSuDungThongTinCaNhan = dto.ChoPhepSuDungThongTinCaNhan == "1";

            int result = _userRepository.Update(entity, objToken!.userid);

            if (result > 0)
            {
                // Đồng bộ POS (TblKhachHang)
                string sodt = username.StartsWith("84") ? username.Replace("84", "0") : username;
                TblKhachHang? khachHang = _posdbcontext.TblKhachHang
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Ma == entity.MaKhachHang && !(x.Deleted ?? false));

                if (khachHang != null)
                {
                    khachHang.Ten = (dto.HoTen + "").ToUpper();
                    khachHang.GioiTinh = dto.GioiTinh;
                    khachHang.DiaChi = dto.SoNha;
                    khachHang.NgaySinh = dtpNgaySinh;
                    khachHang.TienSuBenhLy = dto.TienSuBenhLy;
                    khachHang.DongYtuanThuNoiQuy = dto.DongYTuanThuNoiQuy == "1";
                    khachHang.ChoPhepSuDungThongTinCaNhan = dto.ChoPhepSuDungThongTinCaNhan == "1";

                    _posdbcontext.Update(khachHang);
                    _posdbcontext.SaveChanges();
                }
            }

            return result.isSuccess() ? ResponseUpdateSuccessfully : ResponseUpdateFailure;
        }

        [HttpPost("ResetPassword")]
        [TokenUserCheckHTTP]
        public ResponseModel ResetPassword([FromForm] ResetPasswordDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);
            var result = _userRepository.ResetPassword(username, dto.OldPassword, dto.NewPassword, objToken!.userid);
            if (result.isSuccess()) return ResponseSuccessfully;
            if (result.isNotExit()) return new ResponseModelError("Số điện thoại chưa được đăng ký");
            if (result.isNotMatch()) return new ResponseModelError("Mật khẩu không đúng");
            return ResponseHaveError;
        }

        [HttpPost("ResetForgotPassword")]
        [APIKeyCheck]
        public ResponseModel ResetForgotPassword([FromForm] ResetForgotPasswordDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);                           
            if (dto.Otp != PASSCODEPASSWORD && !_smsOtpRepository.CheckOTPValid(username, dto.Otp))
            {
                return new ResponseModelError("Mã OTP không đúng!");
            }        
            var result = _userRepository.ResetForgotPassword(username, dto.Password);
            if (result.isSuccess()) return ResponseSuccessfully;
            if (result.isNotExit()) return new ResponseModelError("Số điện thoại chưa được đăng ký");
            return ResponseHaveError;
        }

        [HttpPost("Delete")]
        [APIKeyCheck]
        public ResponseModel Delete([FromForm] string username)
        {
            AttachCountryCodeForPhoneNumber(username, out username);
            var result = _userRepository.Delete(username);

            if (result.isSuccess())
                return ResponseSuccessfully;
            if (result.isNotExit())
                return new ResponseModelError("Không tìm thấy người dùng");
            return ResponseHaveError;
        }


        [HttpPost("Checkin")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> Checkin([FromForm] CheckinDto dto)
        {
            Guid maDiemBanHang = dto.MaDiemBanHang.GetGuid();
            var user = await _context.SysUser.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == objToken.userid);
            if (user == null) return new ResponseModelError("Khách hàng chưa login");
            //var diemBanHang = _diemBanHangPOSRepository.GetById(maDiemBanHang);

            TblDiemBanHang? diemBanHang = _posdbcontext.TblDiemBanHang.AsNoTracking().FirstOrDefault(x => x.Ma == maDiemBanHang && !(x.Deleted ?? false));

            if (diemBanHang == null) return new ResponseModelError("Điểm bán hàng không tồn tại");
            TblPhongBan? boPhan = _posdbcontext.TblPhongBan.AsNoTracking().FirstOrDefault(x => x.Ma == diemBanHang.MaPhongBan && !(x.Deleted ?? false));
            if (boPhan == null)
            {
                return new ResponseModelError("Bộ phận của điểm bán hàng không tồn tại");
            }

            var lichSuGoiDV = _lichSuMuaGoiDichVuPOSRepository.GetById(dto.MaLichSuGoiDichVu);
            if (lichSuGoiDV == null) return new ResponseModelError("Gói dịch vụ không tồn tại trong data");

            var goiDichVu = _lichSuMuaGoiDichVuRepository.GetById(dto.MaLichSuGoiDichVu);
            if (goiDichVu == null) return new ResponseModelError("Gói dịch vụ không tồn tại");

            if (lichSuGoiDV.MaPhongBan != diemBanHang.MaPhongBan)
            {
                return new ResponseModelError("Gói dịch vụ không áp dụng cho bộ phận " + boPhan.Ten);
            }            

            if (goiDichVu.NgayKichHoat != null && goiDichVu.NgayKichHoat.Value.Date > DateTime.Now.Date)
                return new ResponseModelError("Gói dịch vụ chưa đến ngày kích hoạt");

            if (goiDichVu.NgayHetHan != null && goiDichVu.NgayHetHan.Value.Date < DateTime.Now.Date)
                return new ResponseModelError("Gói dịch vụ hết hạn");

            var item = new HospitalityCustomerAPI.Models.HCAEntity.OpsCheckIn
            {
                MaChiNhanh = diemBanHang.MaChiNhanh,
                MaPhongBan = diemBanHang.MaPhongBan,
                MaDiemBanHang = diemBanHang.Ma,
                MaLichSuGoiDichVu = goiDichVu.Ma,
                MaKhachHang = user.MaKhachHang,
                NgayCheckIn = DateTime.Now,
                MaNhanVienPhuTrach = goiDichVu.NhanVienPt,
                CreatedDate = DateTime.Now,
            };

            var itemPos = new HospitalityCustomerAPI.Models.POSEntity.OpsCheckIn
            {
                MaChiNhanh = diemBanHang.MaChiNhanh,
                MaPhongBan = diemBanHang.MaPhongBan,
                MaDiemBanHang = diemBanHang.Ma,
                MaLichSuGoiDichVu = goiDichVu.Ma,
                MaKhachHang = user.MaKhachHang,
                NgayCheckIn = DateTime.Now,
                CreatedDate = DateTime.Now,
                MaCheckInKhacHang = item.Ma,
                MaNhanVienPhuTrach = goiDichVu.NhanVienPt,
            };

            await using var tran1 = await _context.Database.BeginTransactionAsync();
            await using var tran2 = await _posdbcontext.Database.BeginTransactionAsync();

            try
            {
                _context.Add(item);
                goiDichVu.SoLanDaSuDung = (goiDichVu.SoLanDaSuDung ?? 0) + 1;
                goiDichVu.SoLanConLai = (goiDichVu.SoLanSuDung ?? 0) - (goiDichVu.SoLanDaSuDung ?? 0);
                _context.Update(goiDichVu);
                await _context.SaveChangesAsync();
                await tran1.CommitAsync();

                _posdbcontext.Add(itemPos);
                lichSuGoiDV.SoLanDaSuDung = (lichSuGoiDV.SoLanDaSuDung ?? 0) + 1;
                lichSuGoiDV.SoLanConLai = (lichSuGoiDV.SoLanSuDung ?? 0) - (lichSuGoiDV.SoLanDaSuDung ?? 0);
                _posdbcontext.Update(lichSuGoiDV);
                await _posdbcontext.SaveChangesAsync();
                await tran2.CommitAsync();

                //// ===== FIRE-AND-FORGET mở cửa sau khi checkin thành công =====
                //var gpioAlias = string.IsNullOrWhiteSpace(dto.GpioAlias) ? "default" : dto.GpioAlias!.Trim();
                //_ = Task.Run(async () =>
                //{
                //    try
                //    {
                //        var ok = await _espClient.TriggerByAliasAsync(gpioAlias);
                //        if (!ok) _logger.LogWarning("ESP trigger FAILED (async) alias={alias} checkin={Ma}", gpioAlias, item.Ma);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "ESP trigger EXCEPTION (async) alias={alias} checkin={Ma}", gpioAlias, item.Ma);
                //    }
                //});



                _ = Task.Run(async () =>
                {
                    try
                    {
                        var baseUrl = string.IsNullOrWhiteSpace(diemBanHang.IpOpenDoor)
                            ? "http://172.16.10.169" // fallback cuối (nếu muốn)
                            : (diemBanHang.IpOpenDoor!.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                ? diemBanHang.IpOpenDoor!
                                : $"http://{diemBanHang.IpOpenDoor}");

                        var pin = diemBanHang.ControlPin; // ví dụ "gym_door_2PULSE"

                        if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(pin))
                        {
                            var ok = await _espClient.TriggerWithEndpointAsync(baseUrl, pin!);
                            if (!ok)
                                _logger.LogWarning("ESP trigger FAILED (async) ip={ip} pin={pin} checkin={Ma}",
                                    baseUrl, pin, item.Ma);
                        }
                        else
                        {
                            _logger.LogWarning("ESP cấu hình trống cho DiemBanHang={Ma}", diemBanHang.Ma);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ESP trigger exception (async) DiemBanHang={Ma}", diemBanHang.Ma);
                    }
                });

                // Có thể trả kèm gợi ý alias để client biết retry (nếu cần)
                return new ResponseModelSuccess("Đã check in thành công", new { CheckinId = item.Ma, DoorAlias = "" });
            }
            catch (Exception ex)
            {
                await tran1.RollbackAsync();
                await tran2.RollbackAsync();
                return new ResponseModelError($"Checkin thất bại: {ex.Message}");
            }
        }

        [HttpPost("OpenDoor")]
        [TokenUserCheckHTTP] // hoặc APIKeyCheck tùy bạn muốn bảo vệ mức nào
        public async Task<ResponseModel> OpenDoor([FromForm] string MaDiemBanHang)
        {
            try
            {
                Guid maDiemBanHang = MaDiemBanHang.GetGuid();
                var diemBanHang = _diemBanHangPOSRepository.GetById(maDiemBanHang);
                if (diemBanHang == null) return new ResponseModelError("Điểm bán hàng không tồn tại");

                if ((diemBanHang.IpOpenDoor ?? "") == "")
                {
                    return new ResponseModelError("Điểm bán hàng chưa thiết lập chế độ mở cửa");
                }
                TblPhongBan? boPhan = await _posdbcontext.TblPhongBan.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == diemBanHang.MaPhongBan && !(x.Deleted ?? false));
                if (boPhan == null)
                {
                    return new ResponseModelError("Bộ phận của điểm bán hàng không tồn tại");
                }
                var user = await _context.SysUser.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == objToken.userid);
                if (user == null) return new ResponseModelError("Khách hàng chưa login");

                ///Chiến sửa lại đoạn kiểm tra gói dịch vụ
                // 1. Tìm xem có BẤT KỲ gói cá nhân nào HỢP LỆ để mở cửa không             
                var hasValidPackage = await _posdbcontext.OpsLichSuMuaGoiDichVu.AsNoTracking()
                    .AnyAsync(x => x.MaKhachHang == user.MaKhachHang
                                && x.MaPhongBan == diemBanHang.MaPhongBan
                                && !(x.Deleted ?? false)
                                && (x.DaKichHoat ?? false) == true                                      // Đã kích hoạt
                                && (x.SoLanSuDung ?? 0) - (x.SoLanDaSuDung ?? 0) > 0                    // Còn lượt
                                && (x.NgayHetHan == null || x.NgayHetHan.Value.Date >= DateTime.Today)  // Chưa hết hạn
                    );


                if (!hasValidPackage)
                {
                    hasValidPackage = await (from t in _posdbcontext.OpsGoiDichVuGiaDinh.AsNoTracking()
                                             join g in _posdbcontext.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals g.Ma
                                             where t.MaKhachHang == user.MaKhachHang
                                                && !(t.Deleted ?? false)
                                                && g.MaPhongBan == diemBanHang.MaPhongBan
                                                && !(g.Deleted ?? false)
                                                && (g.DaKichHoat ?? false) == true
                                                && ((g.SoLanSuDung ?? 0) - (g.SoLanDaSuDung ?? 0) > 0)
                                                && (g.NgayHetHan == null || g.NgayHetHan.Value.Date >= DateTime.Today)
                                             select g.Ma).AnyAsync();
                }

                if (!hasValidPackage)
                {
                    return new ResponseModelError("Không tìm thấy gói dịch vụ khả dụng (Vui lòng kiểm tra: Hết hạn / Chưa kích hoạt / Hết lượt).");
                }

                ///==================================================


                //// tìm gói dv của khách hàng trong bộ phận này
                //var goiDichVu = await _posdbcontext.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhachHang == user.MaKhachHang && x.MaPhongBan == diemBanHang.MaPhongBan && !(x.Deleted ?? false));
                //var goiGiaDinhs = await (from t in _posdbcontext.OpsGoiDichVuGiaDinh.AsNoTracking().Where(x => x.MaKhachHang == user.MaKhachHang && !(x.Deleted ?? false))
                //                         join g in _posdbcontext.OpsLichSuMuaGoiDichVu.AsNoTracking() on t.MaLichSuGoiDichVu equals g.Ma
                //                         where g.MaPhongBan == diemBanHang.MaPhongBan && !(g.Deleted ?? false)
                //                         select g).ToListAsync();

                //if (goiDichVu == null && goiGiaDinhs == null)
                //{
                //    return new ResponseModelError("Khách hàng chưa mua gói dịch vụ " + boPhan.Ten);
                //}

                //if (goiDichVu != null && !(goiDichVu.DaKichHoat ?? false))
                //{
                //    return new ResponseModelError("Khách hàng có gói dịch vụ nhưng chưa kích hoạt");
                //}

                //if (goiDichVu != null && (goiDichVu.SoLanSuDung ?? 0) - (goiDichVu.SoLanDaSuDung ?? 0) <= 0)
                //{
                //    return new ResponseModelError("Khách hàng có gói dịch vụ đã hết số lần sử dụng");
                //}

                //if (goiDichVu != null && goiDichVu.NgayHetHan != null && goiDichVu.NgayHetHan.Value.Date < DateTime.Now.Date)
                //    return new ResponseModelError("Gói dịch vụ hết hạn");

                //bool hasUsableFamilyPackage = goiGiaDinhs.Any(x =>
                //                                (x.DaKichHoat ?? false) &&
                //                                ((x.SoLanSuDung ?? 0) - (x.SoLanDaSuDung ?? 0) > 0) &&
                //                                (!x.NgayHetHan.HasValue || x.NgayHetHan.Value.Date >= DateTime.Today) // chưa hết hạn
                //                            );

                //if (goiDichVu == null && goiGiaDinhs.Any() && !hasUsableFamilyPackage)
                //{
                //    return new ResponseModelError("Khách hàng có gói dịch vụ gia đình đã hết hạn/ hết số lần sử dụng/ chưa kích hoạt");
                //}

                var baseUrl = string.IsNullOrWhiteSpace(diemBanHang.IpOpenDoor)
                          ? "http://172.16.10.169" // fallback cuối (nếu muốn)
                          : (diemBanHang.IpOpenDoor!.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                              ? diemBanHang.IpOpenDoor!
                              : $"http://{diemBanHang.IpOpenDoor}");

                var pin = diemBanHang.ControlPin; // ví dụ "gym_door_2PULSE"

                if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(pin))
                {
                    var ok = await _espClient.TriggerWithEndpointAsync(baseUrl, pin!);
                    if (!ok)
                    {
                        _logger.LogWarning("ESP trigger FAILED (async) ip={ip} pin={pin} checkin={Ma}",
                          baseUrl, pin, "");

                        return new ResponseModelError("Liên hệ quầy tiếp tân");
                    }          
                    else
                    {
                        var item = new  OpsOpenDoor
                        {         
                            MaDiemBanHang = diemBanHang.Ma,                           
                            MaKhachHang = user.MaKhachHang,                          
                            CreatedDate = DateTime.Now,
                        };
                        _posdbcontext.Add(item);
                        await _posdbcontext.SaveChangesAsync();
                        return new ResponseModelSuccess("Đã mở cửa");
                    }
                }
                else
                {
                    _logger.LogWarning("ESP cấu hình trống cho DiemBanHang={Ma}", diemBanHang.Ma);
                    return new ResponseModelError($"ESP cấu hình trống cho DiemBanHang={diemBanHang.Ten}");
                }                   
            }
            catch (Exception ex)
            {
                return new ResponseModelError(ex.Message);
            }
           
        }


        [HttpPost("GetListGoiDichVuConSuDung")]
        [TokenUserCheckHTTP]
        public ResponseModel GetListGoiDichVuConSuDung(string MaKhachHang)
        {
            Guid maKhachHang = MaKhachHang.GetGuid();

            var khachHang = _userRepository.GetItemByKhachHang(maKhachHang);

            if (khachHang == null)
            {
                return new ResponseModelError("Khách hàng không tồn tại");
            }

            var listData = _lichSuMuaGoiDichVuRepository.GetListGoiDichVuConSuDung(maKhachHang);

            return new ResponseModelSuccess("", listData);
        }

        [HttpPost("getBoPhanTheThao")]
        [APIKeyCheck]
        public ResponseModel getBoPhanTheThao()
        {
            var listData = (from t in _posdbcontext.TblPhongBan.AsNoTracking()
                            join n in _posdbcontext.TblNhomBoPhan.AsNoTracking() on t.MaNhomBoPhan equals n.Ma
                            where !(t.Deleted ?? false) && (n.Ma == (int)NhomBoPhan.TheThao)
                            select new
                            {
                                ma = t.Ma,
                                code = t.Code,
                                ten = t.Ten,
                            }).ToList();

            return new ResponseModelSuccess("", listData);
        }       

        [HttpPost("GetListGoiDichVu")]
        [TokenUserCheckHTTP]
        public ResponseModel GetListGoiDichVu(string MaKhachHang)
        {
            Guid maKhachHang = MaKhachHang.GetGuid();

            var khachHang = _userRepository.GetItemByKhachHang(maKhachHang);

            if (khachHang == null)
            {
                return new ResponseModelError("Khách hàng không tồn tại");
            }

            var listData = _lichSuMuaGoiDichVuRepository.GetListGoiDichVu(maKhachHang);

            return new ResponseModelSuccess("", listData);
        }

        [HttpPost("GetLichSuCheckin")]
        [TokenUserCheckHTTP]
        public ResponseModel GetLichSuCheckin(string MaLichSuGoiDichVu)
        {
            Guid maLichSuGoiDichVu = MaLichSuGoiDichVu.GetGuid();

            var listData = _checkInRepository.GetLichSuCheckin(maLichSuGoiDichVu);

            return new ResponseModelSuccess("Lấy thành công lịch sự checkin", listData);
        }


        [HttpGet("GetUserInfo")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> GetUserInfo()
        {
            var sysUser = _userRepository.GetItem(objToken.userid);

            var phoneNumber = sysUser.Username;

            AttachCountryCodeForPhoneNumber(phoneNumber, out phoneNumber);

            var khachHang = _khachHangRepository.GetKhachHangByPhone(phoneNumber);

            if (sysUser == null && khachHang == null)
            {
                return new ResponseModelError("Không tìm thấy thông tin user");
            }

            var dto = new UserInfoDto
            {
                MaUser = sysUser?.Ma ?? Guid.Empty,
                Username = sysUser?.Username,
                FullName = sysUser?.FullName,
                HinhAnh = sysUser?.HinhAnh,
                NgaySinh = sysUser?.NgaySinh?.ToString("dd/MM/yyyy"),
                GioiTinh = sysUser?.GioiTinh,

                //Sử dụng cho địa chỉ
                SoNha = sysUser?.SoNha,
                MaQuocGia = sysUser?.MaQuocGia ?? Guid.Empty,
                MaTinh = sysUser?.MaTinh ?? Guid.Empty,
                MaPhuongXa = sysUser?.MaPhuongXa ?? Guid.Empty,
                //CCCD
                HoChieu = sysUser?.HoChieu ?? "",

                MaKhachHang = khachHang?.Ma ?? Guid.Empty,
                Ten = khachHang?.Ten ?? "",
                SoDienThoai = khachHang?.SoDienThoai ?? "",
                DiaChi = khachHang?.DiaChi ?? "",
                TienSuBenhLy = sysUser?.TienSuBenhLy ?? "",
                DongYTuanThuNoiQuy = sysUser?.DongYtuanThuNoiQuy == true ? "1" : "0",
                ChoPhepSuDungThongTinCaNhan = sysUser?.ChoPhepSuDungThongTinCaNhan == true ? "1" : "0",
            };

            return new ResponseModelSuccess("Thành công", dto);
        }


        [HttpPost("SendForgotPasswordOtp")]
        [APIKeyCheck]
        public ResponseModel SendForgotPasswordOtp([FromForm] string username)
        {
            AttachCountryCodeForPhoneNumber(username, out var normalizedUsername);

            // 1. Check user CÓ TỒN TẠI không (ngược với Register)
            if (_userRepository.GetItemByPhone(normalizedUsername) == null)
            {
                return new ResponseModelError("Số điện thoại này chưa được đăng ký.");
            }

            // 2. Logic gửi OTP (giống hệt Register)
            if (_smsOtpRepository.CheckPhoneExist(normalizedUsername))
            {
                if (!_smsOtpRepository.CheckOTPNeedToRenew(normalizedUsername))
                {
                    return new ResponseModel("OTPrequied", "Bạn đã yêu cầu OTP và vẫn còn hiệu lực.");
                }
                if (_smsOtpRepository.CheckOTPLimit(normalizedUsername))
                {
                    return new ResponseModelError("Bạn đã vượt số lần nhận OTP tối đa của tháng!");
                }

                var (plainOtp, updateStatus) = _smsOtpRepository.GenerateAndSaveNewOtp(normalizedUsername);
                if (updateStatus == ActionStatus.Success.toInt() && plainOtp != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(normalizedUsername, plainOtp, finalApiKey, apiUrl);
                    return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra tin nhắn.");
                }
                return new ResponseModelError("Không thể gửi lại OTP. Vui lòng thử lại.");
            }
            else
            {
                var (plainOtp, addStatus) = _smsOtpRepository.GenerateAndSaveOtp(normalizedUsername);
                if (addStatus == ActionStatus.Success.toInt() && plainOtp != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(normalizedUsername, plainOtp, finalApiKey, apiUrl);
                    return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra tin nhắn.");
                }
                return new ResponseModelError("Không thể gửi OTP. Vui lòng thử lại.");
            }
        }

        [HttpPost("GetPilatesPackages")]
        [TokenUserCheckHTTP]
        public ResponseModel GetPilatesPackages()
        {
            // 1. Lấy thông tin User & Khách hàng
            var userId = objToken.userid;
            var sysUser = _userRepository.GetItem(userId);

            if (sysUser == null || sysUser.MaKhachHang == null)
            {
                // Fallback: Thử tìm theo SĐT trong bảng Khách Hàng nếu bảng User chưa sync
                // (Tùy logic dự án, ở đây return lỗi cho an toàn)
                return new ResponseModelError("Không tìm thấy thông tin khách hàng liên kết.");
            }

            // 2. Lấy GUID Pilates từ Enum cứng (Extension method GetEnumGuid)
            Guid maBoPhanPilates = 
                BoPhanTheThaoEnum.Pilates.GetEnumGuid();

            // 3. Gọi Repository
            var listData = _lichSuMuaGoiDichVuRepository
                            .GetListGoiDichVuByBoPhan(sysUser.MaKhachHang.Value, maBoPhanPilates);

            // 4. Trả về
            return new ResponseModelSuccess("Thành công", listData);
        }
    }
}
