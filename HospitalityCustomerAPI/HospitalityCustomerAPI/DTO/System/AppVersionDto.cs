namespace HospitalityCustomerAPI.DTO.System
{
    // DTO trả về cho Client (Mobile App)
    public sealed class AppVersionClientResponseDto
    {
        public string Version { get; set; }
        public bool IsForceUpdate { get; set; }
        public string Description { get; set; }
        public string StoreUrl { get; set; }
    }

    // DTO để Admin/Postman gửi lên update
    public sealed class UpdateAppVersionDto
    {
        public string AndroidVersionRaw { get; set; }
        public string IosVersionRaw { get; set; }
        public string Description { get; set; }
        public string UpdatedBy { get; set; }
    }
}