using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.User;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class UserController : ApiControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISmsOtpRepository _smsOtpRepository;

        public UserController(
               HungDuyHospitalityCustomerContext context,
               IUserRepository userRepository,
               ISmsOtpRepository smsOtpRepository
           ) : base(context)
        {
            _userRepository = userRepository;
            _smsOtpRepository = smsOtpRepository;
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
    }
}
