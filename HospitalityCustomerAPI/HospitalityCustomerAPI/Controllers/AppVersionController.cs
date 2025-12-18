using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.System;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class AppVersionController : ApiControllerBase
    {
        // --- 1. KHAI BÁO LINK CỨNG TẠI ĐÂY ---
        // Bạn nhớ thay link thật của App mình vào nhé
        private const string URL_STORE_ANDROID = "https://play.google.com/store/apps/details?id=hungduy.com.vn.hungduy_service";
        private const string URL_STORE_IOS = "https://apps.apple.com/us/app/cana-hospitality/id6752832098";

        private readonly HungDuyHospitalityCustomerContext _context;

        public AppVersionController(HungDuyHospitalityCustomerContext context) : base(context)
        {
            _context = context;
        }

        [HttpGet("GetLatestVersion")]
        [APIKeyCheck]
        public ResponseModel GetLatestVersion([FromQuery] string platform)
        {
            if (string.IsNullOrEmpty(platform))
            {
                return new ResponseModelError("Vui lòng truyền tham số platform (android/ios)");
            }

            var versionConfig = _context.SysAppVersion.FirstOrDefault();
            if (versionConfig == null)
            {
                return new ResponseModelError("Hệ thống chưa có cấu hình phiên bản");
            }

            string rawVersionString = "";
            string storeUrl = ""; // Biến tạm để hứng link

            // --- 2. XỬ LÝ GÁN LINK THEO PLATFORM ---
            if (platform.ToLower().Trim() == "android")
            {
                rawVersionString = versionConfig.AndroidVer;
                storeUrl = URL_STORE_ANDROID; // Gán link Android
            }
            else if (platform.ToLower().Trim() == "ios")
            {
                rawVersionString = versionConfig.IosVer;
                storeUrl = URL_STORE_IOS;     // Gán link iOS
            }
            else
            {
                return new ResponseModelError("Platform không hợp lệ");
            }

            // Logic tách chuỗi (giữ nguyên)
            bool isForce = false;
            string cleanVersion = "1.0.0";

            if (!string.IsNullOrEmpty(rawVersionString) && rawVersionString.Contains(":"))
            {
                var parts = rawVersionString.Split(':');
                if (parts.Length >= 2)
                {
                    isForce = parts[0] == "1";
                    cleanVersion = parts[1];
                }
            }
            else
            {
                cleanVersion = rawVersionString ?? "1.0.0";
            }

            // --- 3. TRẢ VỀ KÈM STORE URL ---
            var responseDto = new AppVersionClientResponseDto
            {
                Version = cleanVersion,
                IsForceUpdate = isForce,
                Description = versionConfig.Description ?? "Vui lòng cập nhật phiên bản mới.",
                StoreUrl = storeUrl // Trả về link đã gán ở trên
            };

            return new ResponseModelSuccess("Lấy thông tin thành công", responseDto);
        }

        [HttpPost("UpdateVersion")]
        [APIKeyCheck]
        public ResponseModel UpdateVersion([FromBody] UpdateAppVersionDto dto)
        {
            var config = _context.SysAppVersion.FirstOrDefault();

            if (config == null)
            {
                config = new SysAppVersion();
                config.Id = Guid.NewGuid();
                _context.SysAppVersion.Add(config);
            }

            if (!string.IsNullOrEmpty(dto.AndroidVersionRaw))
                config.AndroidVer = dto.AndroidVersionRaw;

            if (!string.IsNullOrEmpty(dto.IosVersionRaw))
                config.IosVer = dto.IosVersionRaw;

            if (!string.IsNullOrEmpty(dto.Description))
                config.Description = dto.Description;

            config.LastUpdatedTime = DateTime.Now;
            config.LastUpdatedBy = dto.UpdatedBy ?? "System_Admin";

            try
            {
                _context.SaveChanges();
                return new ResponseModelSuccess("Cập nhật phiên bản hệ thống thành công");
            }
            catch (Exception ex)
            {
                return new ResponseModelError($"Lỗi khi lưu dữ liệu: {ex.Message}");
            }
        }
    }
}