using HospitalityCustomerAPI.Common.Attributes;
using System.ComponentModel;

namespace HospitalityCustomerAPI.Common
{
    public enum NhomBoPhan
    {
        TheThao = 1,  
    }

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
        [EnumGuid("E0381822-6392-48AC-BB92-EE6E23D88E82")]
        TinTuc,

        [EnumGuid("7f151c56-efaa-48ba-89d0-bc45ca97645f")]
        ThamMy,
    }

    public enum QuocGia
    {
        [EnumGuid("A47E59D0-6ABD-4E28-8556-9FB83A95F613")]
        VietNam
    }
}
