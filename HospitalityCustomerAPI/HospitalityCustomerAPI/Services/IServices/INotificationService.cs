using HospitalityCustomerAPI.Models.HCAEntity;
using static HospitalityCustomerAPI.Common.Enum;

namespace HospitalityCustomerAPI.Services.IServices
{
    public interface INotificationService
    {
        Task<(bool Success, string MessageId)> SendNotification(string fcmToken, string title, string body);
        Task SaveNotification(string phoneNumber, string title, string body, string fcmToken, string messageId, NotificationType type = NotificationType.ThongBao);
        Task<IEnumerable<SysNotifications>> GetUnreadNotifications(string phoneNumber);
        Task MarkAsRead(Guid notificationId, string phoneNumber);
        Task<int> GetUnreadCount(string phoneNumber);
        Task<IEnumerable<SysNotifications>> GetAllNotifications(string phoneNumber);
        Task MarkAllRead(string phoneNumber);
        Task<object> GetAllNotificationsForAnalysis(DateTime? startDate);
    }
}
