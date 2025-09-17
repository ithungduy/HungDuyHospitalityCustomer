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

        int AddOTP(string phoneNumber);
        int UpdateOTP(string phoneNumber);
        int RemoveOTP(string phoneNumber);
    }
}
