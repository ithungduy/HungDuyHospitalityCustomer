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
               ILogger<UserController> logger
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
            if (isDev)
            {
                return CreateUserAndSyncPos(dto, username);
            }

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
                    if (_smsOtpRepository.UpdateOTP(username) == ActionStatus.Success.toInt())
                    {
                        return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra SMS.");
                    }
                    return new ResponseModelError("Không thể gửi lại OTP. Vui lòng thử lại.");
                }
                else
                {
                    if (_smsOtpRepository.AddOTP(username) == ActionStatus.Success.toInt())
                    {
                        return new ResponseModel("OTPrequied", "Đã gửi OTP. Vui lòng kiểm tra SMS.");
                    }
                    return new ResponseModelError("Không thể gửi OTP. Vui lòng thử lại.");
                }
            }

            // 2) ĐÃ có OTP → kiểm tra hợp lệ
            if (!_smsOtpRepository.CheckOTPValid(username, dto.Otp!))
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

            var result = _userRepository.Update(entity, objToken!.userid);
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
            if (!_smsOtpRepository.CheckOTPValid(username, dto.Otp))
                return new ResponseModelError("Mã OTP không đúng!");

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

            var diemBanHang = _diemBanHangPOSRepository.GetById(maDiemBanHang);
            if (diemBanHang == null) return new ResponseModelError("Điểm bán hàng không tồn tại");

            var lichSuGoiDV = _lichSuMuaGoiDichVuPOSRepository.GetById(dto.MaLichSuGoiDichVu);
            if (lichSuGoiDV == null) return new ResponseModelError("Gói dịch vụ không tồn tại trong data");

            var goiDichVu = _lichSuMuaGoiDichVuRepository.GetById(dto.MaLichSuGoiDichVu);
            if (goiDichVu == null) return new ResponseModelError("Gói dịch vụ không tồn tại");
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

                // ===== FIRE-AND-FORGET mở cửa sau khi checkin thành công =====
                var gpioAlias = string.IsNullOrWhiteSpace(dto.GpioAlias) ? "default" : dto.GpioAlias!.Trim();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var ok = await _espClient.TriggerByAliasAsync(gpioAlias);
                        if (!ok) _logger.LogWarning("ESP trigger FAILED (async) alias={alias} checkin={Ma}", gpioAlias, item.Ma);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ESP trigger EXCEPTION (async) alias={alias} checkin={Ma}", gpioAlias, item.Ma);
                    }
                });


                // ===== Sau khi có Ip và Pins thì bật comment này lên =====
                //_ = Task.Run(async () =>
                //{
                //    try
                //    {
                //        var baseUrl = string.IsNullOrWhiteSpace(diemBanHang.EspIpAddress)
                //            ? "http://172.16.10.169" // fallback cuối (nếu muốn)
                //            : (diemBanHang.EspIpAddress!.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                //                ? diemBanHang.EspIpAddress!
                //                : $"http://{diemBanHang.EspIpAddress}");

                //        var pin = diemBanHang.EspPinName; // ví dụ "gym_door_2PULSE"

                //        if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(pin))
                //        {
                //            var ok = await _espClient.TriggerWithEndpointAsync(baseUrl, pin!);
                //            if (!ok)
                //                _logger.LogWarning("ESP trigger FAILED (async) ip={ip} pin={pin} checkin={Ma}",
                //                    baseUrl, pin, item.Ma);
                //        }
                //        else
                //        {
                //            _logger.LogWarning("ESP cấu hình trống cho DiemBanHang={Ma}", diemBanHang.Ma);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, "ESP trigger exception (async) DiemBanHang={Ma}", diemBanHang.Ma);
                //    }
                //});

                // Có thể trả kèm gợi ý alias để client biết retry (nếu cần)
                return new ResponseModelSuccess("Đã check in thành công", new { CheckinId = item.Ma, DoorAlias = gpioAlias });
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
        public async Task<ResponseModel> OpenDoor([FromForm] string? gpioAlias = null)
        {
            var alias = string.IsNullOrWhiteSpace(gpioAlias) ? "default" : gpioAlias!.Trim();

            var ok = await _espClient.TriggerByAliasAsync(alias);
            if (!ok)
                return new ResponseModelError("Không thể mở cửa. Vui lòng thử lại hoặc liên hệ quầy.");

            return new ResponseModelSuccess("Đã kích hoạt mở cửa", new { DoorAlias = alias });
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
                DiaChi = khachHang?.DiaChi ?? ""
            };

            return new ResponseModelSuccess("Thành công", dto);
        }

    }
}
