using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
// using Microsoft.AspNetCore.Mvc; // không dùng ở repository -> có thể bỏ

namespace HospitalityCustomerAPI.Repositories
{
    public class SmsOtpRepository : ISmsOtpRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;
        private static readonly int otpLength = 6;
        private static readonly int smsLimit = 5;

        private static string NormalizeHash(string s)
        {
            if (s == null) return "";
            // 1) Chuẩn hoá Unicode
            s = s.Normalize(NormalizationForm.FormC);
            // 2) Loại toàn bộ whitespace (space, \r, \n, \t) và zero-width
            s = Regex.Replace(s, @"[\s\u200B-\u200D\uFEFF]", "");
            // 3) Trim đề phòng CHAR padding
            s = s.Trim();
            // 4) Đồng nhất HOA để so sánh
            return s.ToUpperInvariant();
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            // So sánh constant-time
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        public SmsOtpRepository(HungDuyHospitalityCustomerContext context)
        {
            _context = context;
        }

        public SysSmsOtp? GetItem(Guid id)
        {
            return _context.Set<SysSmsOtp>().FirstOrDefault(t => t.Ma == id);
        }

        public SysSmsOtp? GetItemByPhone(string phoneNumber)
        {
            return _context.Set<SysSmsOtp>().FirstOrDefault(t => t.Sdt == phoneNumber);
        }
        // using System.Globalization;

        public bool CheckOTPValid(string phoneNumber, string otp)
        {
            // làm sạch input nhẹ
            otp = (otp ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(otp)) return false;

            var s = GetItemByPhone(phoneNumber);
            if (s == null || s.CreateDate == null || string.IsNullOrEmpty(s.Otp))
                return false;

            // Ép CreateDate về UTC một cách an toàn:
            // - Nếu Kind=Unspecified: coi như UTC (vì ta đã lưu UTC ở Add/Update)
            // - Nếu Kind=Local: chuyển sang UTC
            // - Nếu Kind=Utc: giữ nguyên
            DateTime createUtc = s.CreateDate.Value.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(s.CreateDate.Value, DateTimeKind.Utc),
                DateTimeKind.Local => s.CreateDate.Value.ToUniversalTime(),
                _ => s.CreateDate.Value // Utc
            };

            var nowUtc = DateTime.UtcNow;

            // Hết hạn nếu >= 5 phút
            if (createUtc.AddMinutes(5) <= nowUtc)
                return false;

            // Băm theo CHÍNH CreateDate đã lưu (yyyyMM) để tránh lệch tháng/múi giờ
            var monthToken = createUtc.ToString("yyyyMM", CultureInfo.InvariantCulture);
            var hashToCheck = Utility.GetSHA512(phoneNumber + monthToken + otp);

            // So sánh hash không phân biệt hoa/thường
            var stored = NormalizeHash(s.Otp);
            var computed = NormalizeHash(hashToCheck);

            if (!FixedTimeEquals(stored, computed))
                return false;
            return true;
        }


        public bool CheckOTPNeedToRenew(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s == null || s.CreateDate == null) return true;

            var nowUtc = DateTime.UtcNow;
            var createUtc = DateTime.SpecifyKind(s.CreateDate.Value, DateTimeKind.Utc);
            // cần renew nếu đã quá 5 phút
            return createUtc.AddMinutes(5) < nowUtc;
        }

        public bool CheckPhoneExist(string phoneNumber)
        {
            return GetItemByPhone(phoneNumber) != null;
        }

        public int AddOTP(string phoneNumber)
        {
            string otp = Utility.RandomNumber(otpLength);

            var nowUtc = DateTime.UtcNow;
            var monthToken = nowUtc.ToString("yyyyMM"); // dùng cùng 1 mốc thời gian

            var s = new SysSmsOtp
            {
                Ma = Guid.NewGuid(),
                Sdt = phoneNumber,
                CreateDate = nowUtc,                                  // LƯU UTC
                Otp = Utility.GetSHA512(phoneNumber + monthToken + otp),
                NumberRequest = 1
            };

            _context.SysSmsOtp.Add(s);
            if (_context.SaveChanges() > 0)
            {
                _ = SMSController.sendOTP(phoneNumber, otp, "5");    // gửi theo template
                return ActionStatus.Success.toInt();
            }
            return ActionStatus.Error.toInt();
        }
        public int UpdateOTP(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s == null) return ActionStatus.NotExit.toInt();

            string otp = Utility.RandomNumber(otpLength);

            var nowUtc = DateTime.UtcNow;
            var monthToken = nowUtc.ToString("yyyyMM");

            s.CreateDate = nowUtc;                                   // CẬP NHẬT UTC
            s.Otp = Utility.GetSHA512(phoneNumber + monthToken + otp);
            s.NumberRequest = (s.NumberRequest ?? 0) + 1;

            _context.Update(s);
            if (_context.SaveChanges() > 0)
            {
                _ = SMSController.sendOTP(phoneNumber, otp, "5");
                return ActionStatus.Success.toInt();
            }
            return ActionStatus.Error.toInt();
        }

        public int RemoveOTP(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s == null) return ActionStatus.NotExit.toInt();

            // Đánh dấu hết hạn bằng UTC
            s.CreateDate = DateTime.UtcNow.AddMinutes(-5);

            _context.Update(s);
            return _context.SaveChanges() > 0
                ? ActionStatus.Success.toInt()
                : ActionStatus.Error.toInt();
        }

        public bool CheckOTPLimit(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s == null || s.CreateDate == null) return false;

            var createUtc = DateTime.SpecifyKind(s.CreateDate.Value, DateTimeKind.Utc);
            var nowUtc = DateTime.UtcNow;

            // Giới hạn theo THÁNG (UTC) – ổn định & nhất quán
            return createUtc.Month == nowUtc.Month &&
                   createUtc.Year == nowUtc.Year &&
                   (s.NumberRequest ?? 0) >= smsLimit;
        }
    }
}
