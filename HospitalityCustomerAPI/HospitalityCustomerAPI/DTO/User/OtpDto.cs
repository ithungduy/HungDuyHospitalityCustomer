namespace HospitalityCustomerAPI.DTO.User
{
    public class OtpDto
    {
        public class OtpRequestDto
        {
            public string PhoneNumber { get; set; } = "";
            public string ForceRefresh { get; set; } = "0";
        }

        public class OtpResponseDto
        {
            public string Message { get; set; } = "";
            public bool Success { get; set; }
        }
    }
}
