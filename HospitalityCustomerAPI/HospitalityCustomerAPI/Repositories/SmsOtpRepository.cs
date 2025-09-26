using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
// using Microsoft.AspNetCore.Mvc; // không dùng ở repository -> có thể bỏ

namespace HospitalityCustomerAPI.Repositories
{
    public class SmsOtpRepository : ISmsOtpRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;
        private static readonly int otpLength = 6;
        private static readonly int smsLimit = 3;

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

        public bool CheckOTPValid(string phoneNumber, string otp)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s != null && s.CreateDate != null && s.Otp != null)
            {
                if (s.CreateDate.Value.AddMinutes(5) > DateTime.Now)
                {
                    string hashToCheck = Utility.GetSHA512(phoneNumber + s.CreateDate.Value.ToString("yyyyMM") + otp);
                    return s.Otp.Equals(hashToCheck);
                }
            }
            return false;
        }

        public bool CheckOTPNeedToRenew(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            return s != null && s.CreateDate != null && s.CreateDate.Value.AddMinutes(5) < DateTime.Now;
        }

        public bool CheckPhoneExist(string phoneNumber)
        {
            return GetItemByPhone(phoneNumber) != null;
        }

        public int AddOTP(string phoneNumber)
        {
            string otp = Utility.RandomNumber(otpLength);

            var s = new SysSmsOtp
            {
                Ma = Guid.NewGuid(),
                Sdt = phoneNumber,
                Otp = Utility.GetSHA512(phoneNumber + DateTime.Now.ToString("yyyyMM") + otp),
                CreateDate = DateTime.Now,
                NumberRequest = 1
            };

            _context.SysSmsOtp.Add(s);
            if (_context.SaveChanges() > 0)
            {
                // Gửi theo TEMPLATE (HungDuyHospitality style) – hiệu lực 5 phút
                _ = SMSController.sendOTP(phoneNumber, otp, "5");
                return ActionStatus.Success.toInt();
            }
            return ActionStatus.Error.toInt();
        }

        public int UpdateOTP(string phoneNumber)
        {
            string otp = Utility.RandomNumber(otpLength);
            var s = GetItemByPhone(phoneNumber);
            if (s == null) return ActionStatus.NotExit.toInt();

            s.Otp = Utility.GetSHA512(phoneNumber + DateTime.Now.ToString("yyyyMM") + otp);
            s.CreateDate = DateTime.Now;
            s.NumberRequest = (s.NumberRequest ?? 0) + 1;

            _context.Update(s);
            if (_context.SaveChanges() > 0)
            {
                // Gửi theo TEMPLATE (HungDuyHospitality style) – hiệu lực 5 phút
                _ = SMSController.sendOTP(phoneNumber, otp, "5");
                return ActionStatus.Success.toInt();
            }
            return ActionStatus.Error.toInt();
        }

        public int RemoveOTP(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s == null) return ActionStatus.NotExit.toInt();

            s.CreateDate = DateTime.Now.AddMinutes(-5);

            _context.Update(s);
            return _context.SaveChanges() > 0
                ? ActionStatus.Success.toInt()
                : ActionStatus.Error.toInt();
        }

        public bool CheckOTPLimit(string phoneNumber)
        {
            var s = GetItemByPhone(phoneNumber);
            if (s != null && s.CreateDate != null)
            {
                return s.CreateDate.Value.Month == DateTime.Now.Month &&
                       (s.NumberRequest ?? 0) >= smsLimit;
            }
            return false;
        }
    }
}
