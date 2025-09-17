namespace HospitalityCustomerAPI.DTO.FCM
{
    public class Message
    {
        public string token { get; set; }
        public NotificationContent notification { get; set; }
        public AndroidConfig android { get; set; } // Thêm android config
        public ApnsConfig apns { get; set; }
    }
}
