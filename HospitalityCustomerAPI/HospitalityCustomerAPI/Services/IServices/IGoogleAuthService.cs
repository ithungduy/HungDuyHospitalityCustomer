namespace HospitalityCustomerAPI.Services.IServices
{
    public interface IGoogleAuthService
    {
        Task<string> GetAccessTokenAsync();
    }
}
