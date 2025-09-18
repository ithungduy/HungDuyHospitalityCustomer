using System.ComponentModel;

namespace HospitalityCustomerAPI.Common
{
    public class Enum
    {

        /// <summary>
        /// Loại thông báo
        /// </summary>
        public enum NotificationType
        {
            [Description("Thông báo")]
            ThongBao = 0,

            [Description("Kiểm thử")]
            Test = 1,

            [Description("Tin nhắn bằng fcm")]
            Fcm = 2,

            [Description("Mã xác thực OTP")]
            OTP = 3

        }

        public enum ActionStatus
        {
            Success = 1,
            Error = 0,
            Exist = -1,
            NotExit = -2,
            Disable = -3,
            NotMatch = -4,
            Using = -5,
            TransactionError = -6,
        }

        public enum TinTucEnum
        {
            TinTuc = 2007,
            NoiBat = 1007,
        }
    }
}
