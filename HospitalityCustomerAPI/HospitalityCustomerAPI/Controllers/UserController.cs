using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.User;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class UserController : ApiControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISmsOtpRepository _smsOtpRepository;
        private readonly HungDuyHospitalityContext _posdbcontext;
        private readonly HungDuyHospitalityCustomerContext _context;

        public UserController(
               HungDuyHospitalityCustomerContext context,
               HungDuyHospitalityContext posdbcontext,
               IUserRepository userRepository,
               ISmsOtpRepository smsOtpRepository
           ) : base(context)
        {
            _userRepository = userRepository;
            _smsOtpRepository = smsOtpRepository;
            _posdbcontext = posdbcontext;
            _context = context;
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


        [HttpPost("Checkin")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> Checkin([FromForm] CheckinDto dto)
        {
            Guid maDiemBanHang = dto.MaDiemBanHang.GetGuid();
            TblDiemBanHang? diemBanHang = await _posdbcontext.TblDiemBanHang.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == maDiemBanHang && !(x.Deleted ?? false));
            if (diemBanHang == null)
            {
                return new ResponseModelError("Điểm bán hàng không tồn tại");
            }
            var lichSuGoiDV  = await _posdbcontext.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == dto.MaLichSuGoiDichVu && !(x.Deleted ?? false));
            if (lichSuGoiDV == null)
            {
                return new ResponseModelError("Gói dịch vụ không tồn tại trong data");
            }

            var goiDichVu = await _context.OpsLichSuMuaGoiDichVu.AsNoTracking().FirstOrDefaultAsync(x => x.Ma == dto.MaLichSuGoiDichVu && !(x.Deleted ?? false));
            if (goiDichVu == null)
            {
                return new ResponseModelError("Gói dịch vụ không tồn tại");
            }

            var khachHang = await _context.SysUser.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhachHang == dto.MaKhachHang && !(x.Deleted ?? false));
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
                    goiDichVu.SoLanConLai = (goiDichVu.SoLanConLai ?? 0) - 1;
                    _context.Update(goiDichVu);
                    await _context.SaveChangesAsync();

                    lichSuGoiDV.SoLanDaSuDung = (lichSuGoiDV.SoLanDaSuDung ?? 0) + 1;
                    lichSuGoiDV.SoLanConLai = (lichSuGoiDV.SoLanConLai ?? 0) - 1;
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


        [HttpPost("getListGoiDichVu")]
        [TokenUserCheckHTTP]
        public async Task<ResponseModel> getListGoiDichVu(string MaKhachHang)
        {
            Guid maKhachHang = MaKhachHang.GetGuid(); 
            var khachHang = await _context.SysUser.AsNoTracking().FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang && !(x.Deleted ?? false));
            if (khachHang == null)
            {
                return new ResponseModelError("Khách hàng không tồn tại");
            }

            var listData = await (from t in _context.OpsLichSuMuaGoiDichVu.AsNoTracking().Where(x => x.MaKhachHang == maKhachHang && !(x.Deleted ?? false))
                                  join dv in _context.TblHangHoa.AsNoTracking() on t.MaHangHoa equals dv.Ma
                                  select new
                                  {
                                      maGoiDichVu = t.Ma,
                                      tenGoiDichVu = dv.Ten,
                                      ngayKichHoat = t.CreatedDate,
                                      soLan = t.SoLanDaSuDung ?? 0,
                                      soLanDaSuDung = t.SoLanDaSuDung ?? 0,
                                      conLai = t.SoLanConLai ?? 0,
                                  }).ToListAsync();           

            return new ResponseModelSuccess("",listData);
        }
    }
}
