namespace HospitalityCustomerAPI.DTO.User
{
    public class DeviceDto
    {
        public class UpdateDeviceDto
        {
            public string Username { get; set; } = "";
            public string NewDeviceId { get; set; } = "";
        }

        public class UpdateFcmDto
        {
            public string Username { get; set; } = "";
            public string NewFcm { get; set; } = "";
        }
    }
}
