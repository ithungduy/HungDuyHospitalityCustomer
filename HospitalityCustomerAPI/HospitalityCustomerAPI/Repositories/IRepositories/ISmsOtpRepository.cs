using HospitalityCustomerAPI.Models.HCAEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface ISmsOtpRepository
    {
        SysSmsOtp? GetItem(Guid id);
        SysSmsOtp? GetItemByPhone(string phoneNumber);

        bool CheckOTPValid(string phoneNumber, string otp);
        bool CheckOTPNeedToRenew(string phoneNumber);
        bool CheckPhoneExist(string phoneNumber);
        bool CheckOTPLimit(string phoneNumber);

        // --- THAY ĐỔI CHỮ KÝ ---
        // Trả về (string? Otp, int Status) thay vì chỉ int
        /// <summary>
        /// Tạo, lưu OTP (đã hash) vào DB và trả về mã OTP (plain-text).
        /// </summary>
        /// <returns>Một tuple chứa (string? Otp, int Status). Otp sẽ là null nếu thất bại.</returns>
        (string? Otp, int Status) GenerateAndSaveOtp(string phoneNumber);

        // --- THAY ĐỔI CHỮ KÝ ---
        // Trả về (string? Otp, int Status) thay vì chỉ int
        /// <summary>
        /// Tạo, lưu OTP mới (đã hash) vào DB và trả về mã OTP (plain-text).
        /// </summary>
        /// <returns>Một tuple chứa (string? Otp, int Status). Otp sẽ là null nếu thất bại.</returns>
        (string? Otp, int Status) GenerateAndSaveNewOtp(string phoneNumber);

        // --- GIỮ NGUYÊN ---
        int RemoveOTP(string phoneNumber);
    }
}