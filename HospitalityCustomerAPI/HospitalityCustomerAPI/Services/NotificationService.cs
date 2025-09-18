using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.FCM;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace HospitalityCustomerAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IGoogleAuthService _googleAuthService;
        private readonly HttpClient _httpClient;
        private readonly HungDuyHospitalityCustomerContext _context;
        private const string FCM_URL = "https://fcm.googleapis.com/v1/projects/qlcongviec-fb3e0/messages:send";

        public NotificationService(
            IGoogleAuthService googleAuthService,
            HttpClient httpClient,
            HungDuyHospitalityCustomerContext context
            )
        {
            _googleAuthService = googleAuthService;
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<(bool Success, string MessageId)> SendNotification(string fcmToken, string title, string body)
        {
            try
            {
                // Lấy access token
                var accessToken = await _googleAuthService.GetAccessTokenAsync();

                // Cập nhật notification request để thêm custom sound
                var notification = new FCMNotificationRequest
                {
                    message = new Message
                    {
                        token = fcmToken,
                        notification = new NotificationContent
                        {
                            title = title,
                            body = body
                        },
                        android = new AndroidConfig
                        {
                            notification = new AndroidNotification
                            {
                                sound = "notification_sound2",
                                channel_id = "firebase_messaging_channel"
                            }
                        },
                        apns = new ApnsConfig
                        {
                            payload = new ApnsPayload
                            {
                                aps = new Aps
                                {
                                    sound = "notification_sound2.wav"
                                }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(notification);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Thêm token vào header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Gửi request
                var response = await _httpClient.PostAsync(FCM_URL, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var fcmResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string fullName = fcmResponse.name.ToString();
                    string messageId = fullName.Split('/').Last();
                    return (true, messageId);
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                // Log error
                return (false, null);
            }
        }

        public async Task SaveNotification(
            string phoneNumber,
            string title,
            string body,
            string fcmToken,
            string messageId = null, 
            NotificationType type = NotificationType.ThongBao)
        {
            var notification = new SysNotifications
            {
                UserPhone = phoneNumber,
                Title = title,
                Message = body,
                NotificationType = type.ToString(),
                FcmToken = fcmToken,
                MessageId = messageId,
                IsRead = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.SysNotifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SysNotifications>> GetUnreadNotifications(string phoneNumber)
        {
            return await _context.SysNotifications
                .Where(n => n.UserPhone == phoneNumber &&
                           (!n.IsRead.HasValue || !n.IsRead.Value) &&
                           n.NotificationType != NotificationType.OTP.ToString())
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsRead(Guid notificationId, string phoneNumber)
        {
            var notification = await _context.SysNotifications
                .FirstOrDefaultAsync(n => n.Ma == notificationId && n.UserPhone == phoneNumber);

            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                notification.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCount(string phoneNumber)
        {
            return await _context.SysNotifications
                .CountAsync(n => n.UserPhone == phoneNumber && (!n.IsRead.HasValue || !n.IsRead.Value));
        }

        public async Task<IEnumerable<SysNotifications>> GetAllNotifications(string phoneNumber)
        {
            return await _context.SysNotifications
               .Where(n => n.UserPhone == phoneNumber)
               .OrderByDescending(n => n.CreatedAt)
               .ToListAsync();
        }

        public async Task MarkAllRead(string phoneNumber)
        {
            var notifications = _context.SysNotifications.Where(n =>
                n.UserPhone == phoneNumber &&
                (!n.IsRead.HasValue || !n.IsRead.Value)
            );

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        // Trong NotificationService.cs
        public async Task<object> GetAllNotificationsForAnalysis(DateTime? startDate)
        {
            // Bắt đầu từ tất cả thông báo
            IQueryable<SysNotifications> query = _context.SysNotifications;

            // Thêm điều kiện lọc theo ngày nếu có
            if (startDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            }

            // Đếm tổng số bản ghi thỏa mãn điều kiện
            int totalCount = await query.CountAsync();

            // Lấy tất cả thông báo trong khoảng thời gian
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Ma,
                    n.UserPhone,
                    n.Title,
                    n.Message,
                    n.NotificationType,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt,
                    n.UpdatedAt,
                    n.MessageId,
                    n.FcmToken
                })
                .ToListAsync();

            // Đóng gói kết quả với định dạng phù hợp
            var result = new
            {
                TotalCount = totalCount,
                Data = notifications
            };

            return result;
        }

    }
}
