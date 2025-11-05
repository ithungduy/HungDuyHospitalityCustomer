namespace HospitalityCustomerAPI.Common
{
    public interface IOtpNotificationService
    {
        Task SendOtpAsync(string sdt, string otp, string apiKey, string apiUrl);
    }
}