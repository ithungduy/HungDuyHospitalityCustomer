namespace HospitalityCustomerAPI.DTO.User
{
    public class UserDto
    {
        public class UserLoginDto
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string DeviceID { get; set; } = "";
            public string? FCM { get; set; }
        }

        public class UserResponseDto
        {
            public string Username { get; set; } = "";
            public string AppVersion { get; set; } = "";
            public string Token { get; set; } = "";
            public string DeviceId { get; set; } = "";
        }

    }
}
