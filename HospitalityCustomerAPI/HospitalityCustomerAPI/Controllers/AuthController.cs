using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.User;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using HospitalityCustomerAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static HospitalityCustomerAPI.DTO.User.DeviceDto;
using static HospitalityCustomerAPI.DTO.User.OtpDto;
using static HospitalityCustomerAPI.DTO.User.UserDto;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class AuthController : ApiControllerBase
    {
        private const string PASSCODE = "hcapasscode";
        private static string appver = "1:1.0.0+3";
        private static string messIdSuccess = "";

        private const string PASSCODEFA = "F1n@nc3@cc0nt1ng"; 
        private const string apiUrl = "http://localhost:5202/api/App/SendCanaOtp";
        //private const string apiUrl = "https://fa.hungduy.vn/api/App/SendCanaOtp";

        private readonly HungDuyHospitalityCustomerContext _hungDuyHospitalityCustomerContext;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ISmsOtpRepository _smsOtpRepository;

        private readonly IOtpNotificationService _otpNotificationService;

        public AuthController(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext,
                        INotificationService notificationService,
                        IUserRepository userRepository,
                        ISmsOtpRepository smsOtpRepository,
                        IOtpNotificationService otpNotificationService
                ) : base(hungDuyHospitalityCustomerContext)
        {
            _hungDuyHospitalityCustomerContext = hungDuyHospitalityCustomerContext;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _smsOtpRepository = smsOtpRepository;
            _otpNotificationService = otpNotificationService;

        }

        // --- THÊM HÀM HELPER TẠO KEY ---
        /// <summary>
        /// Tạo API Key động để gọi qua FA
        /// </summary>
        private string GenerateFAGatewayApiKey()
        {
            // Logic này phải khớp với [APIKeyCheckAttribute] bên FA
            return Utility.GetSHA512(PASSCODEFA + DateTime.Now.ToString("yyyyMM"));
        }

        [HttpGet("Mess")]
        public IActionResult Mess([FromQuery] MessDto dto)
        {
            messIdSuccess += dto.MessId;
            return Ok(messIdSuccess);
        }

        [HttpPost("Login")]
        [APIKeyCheck]
        public ResponseModel Login([FromForm] UserLoginDto loginDto)
        {
            AttachCountryCodeForPhoneNumber(loginDto.Username, out var username);
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return new ResponseModelError("Thông tin đăng nhập còn trống");
            }

            var (status, user) = _userRepository.Authenticate(username, loginDto.Password);

            if (status.isSuccess() && user != null)
            {
                user.Fcm = loginDto.FCM ?? "";
                if (string.IsNullOrEmpty(user.Token))
                {
                    user.Token = loginDto.DeviceID;
                }

                _userRepository.Update(user, user.Ma);

                string storedDeviceID = user.Token ?? "";
                var objToken = new UserHCAToken(user.Ma, loginDto.DeviceID);
                string obj = Utility.Base64Encode(JsonConvert.SerializeObject(objToken));
                string sign = Utility.GetSHA256(obj + PASSCODE + DateTime.Now.ToString("yyyyMM"));
                string token = obj + "." + sign;
                var spec = _hungDuyHospitalityCustomerContext.SysAppVersion.Select(x => x.Appver).FirstOrDefault() ?? "1:1.0.0";

                var dto = new UserResponseDto
                {
                    Ma = user.Ma,
                    Username = username,
                    AppVersion = spec,
                    Token = token,
                    DeviceId = !string.IsNullOrEmpty(loginDto.DeviceID) ? loginDto.DeviceID : storedDeviceID,
                };

                return new ResponseModelSuccess("Thành công", dto);
            }
            else if (status.isError())
            {
                return new ResponseModelError("Thông tin đăng nhập không chính xác");
            }
            else if (status.isDisable())
            {
                return new ResponseModelError("Tài khoản đã bị vô hiệu hóa");
            }

            return ResponseHaveError;
        }

        [HttpPost("GetOTPRegister")]
        [APIKeyCheck]
        public ResponseModel GetOTPRegister([FromForm] OtpRequestDto request)
        {
            var phone = request.PhoneNumber;
            if (string.IsNullOrWhiteSpace(phone))
                return new ResponseModelError("Null");

            if (phone.Length < 10)
            {
                AttachCountryCodeForPhoneNumber(phone, out phone);
            }
            if (phone.Length > 10)
            {
                return new ResponseModelError("Số điện thoại không đúng định dạng");
            }

            if (_smsOtpRepository.CheckPhoneExist(phone))
            {
                if (request.ForceRefresh == "1")
                {
                    // Logic update khi force
                    var (plainOtp, updateStatus) = _smsOtpRepository.GenerateAndSaveNewOtp(phone);
                    if (updateStatus == ActionStatus.Success.toInt() && plainOtp != null)
                    {
                        string finalApiKey = GenerateFAGatewayApiKey();
                        _ = _otpNotificationService.SendOtpAsync(phone, plainOtp, finalApiKey, apiUrl);
                        return new ResponseModelSuccess("OTP đã được làm mới");
                    }
                }

                if (!_smsOtpRepository.CheckOTPNeedToRenew(phone))
                    return new ResponseModel("Exist", "Bạn vẫn còn mã OTP còn hạn. Vui lòng sử dụng OTP cũ!");

                if (_smsOtpRepository.CheckOTPLimit(phone))
                    return new ResponseModelError("Bạn đã vượt số lần nhận OTP tối đa của tháng!");

                // Logic update khi hết hạn
                var (plainOtpUpdate, status) = _smsOtpRepository.GenerateAndSaveNewOtp(phone);
                if (status == ActionStatus.Success.toInt() && plainOtpUpdate != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(phone, plainOtpUpdate, finalApiKey, apiUrl);
                    return new ResponseModelSuccess("OTP đã được tạo mới");
                }
            }
            else
            {
                // Logic add mới
                var (plainOtpAdd, status) = _smsOtpRepository.GenerateAndSaveOtp(phone);
                if (status == ActionStatus.Success.toInt() && plainOtpAdd != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(phone, plainOtpAdd, finalApiKey, apiUrl);
                    return new ResponseModelSuccess("OTP đã được tạo");
                }
            }           

            return new ResponseModelError("Không thể xử lý OTP");
        }

        [HttpPost("GetOTP")]
        [APIKeyCheck]
        public ResponseModel GetOTP([FromForm] OtpRequestDto request)
        {
            var phone = request.PhoneNumber;
            if (string.IsNullOrWhiteSpace(phone))
                return new ResponseModelError("Null");

            if (phone.Length < 11)
            {
                AttachCountryCodeForPhoneNumber(phone, out phone);
            }
            if (phone.Length > 11)
            {
                return new ResponseModelError("Số điện thoại bạn nhập vào không đúng định dạng!!!");
            }

            var user = _userRepository.GetItemByPhone(phone);
            if (user == null)
            {
                return new ResponseModelError("Số điện thoại chưa đăng kí");
            }

            if (_smsOtpRepository.CheckPhoneExist(phone))
            {
                if (request.ForceRefresh == "1")
                {
                    // Logic update khi force
                    var (plainOtp, updateStatus) = _smsOtpRepository.GenerateAndSaveNewOtp(phone);
                    if (updateStatus == ActionStatus.Success.toInt() && plainOtp != null)
                    {
                        string finalApiKey = GenerateFAGatewayApiKey();
                        _ = _otpNotificationService.SendOtpAsync(phone, plainOtp, finalApiKey, apiUrl);
                        return new ResponseModelSuccess("OTP đã được làm mới");
                    }
                }

                if (!_smsOtpRepository.CheckOTPNeedToRenew(phone))
                    return new ResponseModel("Exist", "Bạn vẫn còn mã OTP còn hạn sử dụng. Vui lòng dùng OTP đó!");

                if (_smsOtpRepository.CheckOTPLimit(phone))
                    return new ResponseModelError("Bạn đã vượt số lần nhận mã OTP tối đa của tháng !!!");

                // Logic update khi hết hạn
                var (plainOtpUpdate, status) = _smsOtpRepository.GenerateAndSaveNewOtp(phone);
                if (status == ActionStatus.Success.toInt() && plainOtpUpdate != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(phone, plainOtpUpdate, finalApiKey, apiUrl);
                    return new ResponseModelSuccess("OTP đã được tạo mới");
                }
            }
            else
            {
                // Logic add mới
                var (plainOtpAdd, status) = _smsOtpRepository.GenerateAndSaveOtp(phone);
                if (status == ActionStatus.Success.toInt() && plainOtpAdd != null)
                {
                    string finalApiKey = GenerateFAGatewayApiKey();
                    _ = _otpNotificationService.SendOtpAsync(phone, plainOtpAdd, finalApiKey, apiUrl);
                    return new ResponseModelSuccess("OTP đã được tạo");
                }
            }           

            return new ResponseModelError("Không thể xử lý OTP");
        }


        [HttpPost("UpdateDevice")]
        [APIKeyCheck]
        public ResponseModel UpdateDevice([FromForm] UpdateDeviceDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);
            var user = _userRepository.GetItemByPhone(username);
            if (user == null)
            {
                return new ResponseModelError("Không tìm thấy thông tin người dùng");
            }

            user.Token = dto.NewDeviceId;
            _userRepository.Update(user, user.Ma);

            return new ResponseModelSuccess("Cập nhật thiết bị thành công");
        }

        [HttpPost("UpdateFCM")]
        [APIKeyCheck]
        public ResponseModel UpdateFCM([FromForm] UpdateFcmDto dto)
        {
            AttachCountryCodeForPhoneNumber(dto.Username, out var username);
            var user = _userRepository.GetItemByPhone(username);
            if (user == null)
            {
                return new ResponseModelError("Không tìm thấy thông tin người dùng");
            }

            user.Fcm = dto.NewFcm;
            _userRepository.Update(user, user.Ma);

            return new ResponseModelSuccess("Cập nhật FCM thành công");
        }

    }
}
