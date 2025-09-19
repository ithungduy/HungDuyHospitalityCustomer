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


        public UserController(
               HungDuyHospitalityCustomerContext context,
               HungDuyHospitalityContext posdbcontext,
               IUserRepository userRepository,
               ISmsOtpRepository smsOtpRepository,
               IKhachHangRepository khachHangRepository,
               ICheckInRepository checkInRepository,
               ILichSuMuaGoiDichVuRepository lichSuMuaGoiDichVuRepository,
               IDiemBanHangPOSRepository diemBanHangPOSRepository,
               ILichSuMuaGoiDichVuPOSRepository lichSuMuaGoiDichVuPOSRepository
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
        }


        [HttpPost("Register")]
        [APIKeyCheck]
        public ResponseModel Register([FromForm] UserRegisterDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);
            if (_userRepository.GetItemByPhone(username) != null)
            {
                return new ResponseModelError("Số điện thoại đã được dùng");
            }

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
                FullName = dto.HoTen,
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

            var khachHang = _posdbcontext.TblKhachHang.AsNoTracking().FirstOrDefault(x => x.SoDienThoai == username && !(x.Deleted ?? false));
            if (khachHang == null)
            {
                TblKhachHang kh = new TblKhachHang
                {
                    Ma = Guid.NewGuid(),
                    SoDienThoai = username,
                    Ten = dto.HoTen.ToUpper(),
                    Code = username,
                    GioiTinh = dto.GioiTinh,
                    DiaChi = dto.SoNha + "",
                    NgaySinh = dtpNgaySinh,
                    Status = true,
                };
                _posdbcontext.Add(kh);
                _posdbcontext.SaveChanges();
            }
            entity.MaKhachHang = khachHang.Ma;
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
        [APIKeyCheck]
        public async Task<ResponseModel> Checkin([FromForm] CheckinDto dto)
        {
            Guid maDiemBanHang = dto.MaDiemBanHang.GetGuid();

            TblDiemBanHang? diemBanHang = _diemBanHangPOSRepository.GetById(maDiemBanHang);
            if (diemBanHang == null)
            {
                return new ResponseModelError("Điểm bán hàng không tồn tại");
            }

            var lichSuGoiDV = _lichSuMuaGoiDichVuPOSRepository.GetById(dto.MaLichSuGoiDichVu);
            if (lichSuGoiDV == null)
            {
                return new ResponseModelError("Gói dịch vụ không tồn tại trong data");
            }

            var goiDichVu = _lichSuMuaGoiDichVuRepository.GetById(dto.MaLichSuGoiDichVu);
            if (goiDichVu == null)
            {
                return new ResponseModelError("Gói dịch vụ không tồn tại");
            }

            var khachHang = _userRepository.GetItemByKhachHang(dto.MaKhachHang);

            if (khachHang == null)
            {
                return new ResponseModelError("Khách hàng không tồn tại");
            }
            HospitalityCustomerAPI.Models.HCAEntity.OpsCheckIn item = new()
            {
                MaChiNhanh = diemBanHang.MaChiNhanh,
                MaPhongBan = diemBanHang.MaPhongBan,
                MaDiemBanHang = diemBanHang.Ma,
                MaLichSuGoiDichVu = goiDichVu != null ? goiDichVu.Ma : null,
                MaKhachHang = khachHang != null ? khachHang.Ma : null,
                NgayCheckIn = DateTime.Now,
            };

            HospitalityCustomerAPI.Models.POSEntity.OpsCheckIn itemPos = new()
            {
                MaChiNhanh = diemBanHang.MaChiNhanh,
                MaPhongBan = diemBanHang.MaPhongBan,
                MaDiemBanHang = diemBanHang.Ma,
                MaLichSuGoiDichVu = goiDichVu != null ? goiDichVu.Ma : null,
                MaKhachHang = khachHang != null ? khachHang.Ma : null,
                NgayCheckIn = DateTime.Now,
            };

            var txOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            using (var scope = new TransactionScope(TransactionScopeOption.Required, txOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    _context.Add(item);
                    await _context.SaveChangesAsync();

                    goiDichVu.SoLanDaSuDung = (goiDichVu.SoLanDaSuDung ?? 0) + 1;
                    goiDichVu.SoLanConLai = (goiDichVu.SoLanSuDung ?? 0) - (goiDichVu.SoLanDaSuDung ?? 0);
                    _context.Update(goiDichVu);
                    await _context.SaveChangesAsync();

                    lichSuGoiDV.SoLanDaSuDung = (lichSuGoiDV.SoLanDaSuDung ?? 0) + 1;
                    lichSuGoiDV.SoLanConLai = (lichSuGoiDV.SoLanSuDung ?? 0) - (lichSuGoiDV.SoLanDaSuDung ?? 0);
                    _posdbcontext.Update(goiDichVu);
                    await _posdbcontext.SaveChangesAsync();

                    _context.Add(itemPos);
                    await _posdbcontext.SaveChangesAsync();

                    scope.Complete();
                    return new ResponseModelSuccess("Đã check in");
                }
                catch (Exception ex)
                {
                    return new ResponseModelError(ex.Message);
                }
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

        [HttpPost("GetListGoiDichVu")]
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
                NgaySinh = sysUser?.NgaySinh.Value.ToString("dd/MM/yyyy"),
                GioiTinh = sysUser?.GioiTinh,

                MaKhachHang = khachHang?.Ma ?? Guid.Empty,
                Ten = khachHang?.Ten,
                SoDienThoai = khachHang?.SoDienThoai,
                DiaChi = khachHang?.DiaChi
            };

            return new ResponseModelSuccess("Thành công", dto);
        }

    }
}
