using HospitalityCustomerAPI.Common.Attributes;
using System.ComponentModel;

namespace HospitalityCustomerAPI.Common
{
    public enum LopHangHoa
    {
        [EnumGuid("DD201355-51D9-F011-8601-000C29665C4C")]
        PILATES,
    }
    public enum NhomBoPhan
    {
        TheThao = 1,  
    }

    /// <summary>
    /// Enum định danh cứng các bộ phận thể thao để query nhanh
    /// </summary>
    public enum BoPhanTheThaoEnum
    {
        //33411205-AE0E-48B7-A402-3EE71BFF0A89
        [EnumGuid("371F8A75-FEB2-F011-96CB-000C2924A9FE")]
        [Description("CANA-PILATES")]
        Pilates,

        [EnumGuid("41E26771-52DB-4CEF-AE77-6BEDA11F4E1A")]
        [Description("CANA-GYM")]
        Gym,

        [EnumGuid("057D25D8-CF95-4CF5-9D2B-3C7B7F133BA1")]
        [Description("CANA-POOL")]
        HoBoi,

        [EnumGuid("3EF69364-2F2B-48C6-A43E-5B28ECFA424C")]
        [Description("CANA-TENNIS")]
        Tennis,

        [EnumGuid("E61402BC-FDE1-418B-B355-9620CABA7283")]
        [Description("CANA-PICKLEBALL")]
        Pickleball
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
