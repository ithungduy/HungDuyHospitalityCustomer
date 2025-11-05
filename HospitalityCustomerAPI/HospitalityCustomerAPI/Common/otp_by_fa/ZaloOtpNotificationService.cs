using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace HospitalityCustomerAPI.Common
{
    public class ZaloOtpNotificationService : IOtpNotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZaloOtpNotificationService> _logger;
        public record SendCanaOtpRequest(string Sdt, string Otp);

        public ZaloOtpNotificationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ZaloOtpNotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpAsync(
            string sdt,
            string otp ,
            string apiKey,
            string apiUrl
            )
        {
            try
            {                          
                var client = _httpClientFactory.CreateClient("ZaloOtpClient");

                // 2. Chuẩn bị request
                var requestBody = new SendCanaOtpRequest(sdt, otp);
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // 3. Thêm API Key vào header
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("token", apiKey);

                // 4. Gửi request
                var response = await client.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Goi API SendCanaOtp that bai. Status: {status}", response.StatusCode);
                }
                // Nếu thành công thì không cần làm gì cả, bên kia đã tự xử lý
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi khi goi SendCanaOtp API");
            }
        }

       
    }
}