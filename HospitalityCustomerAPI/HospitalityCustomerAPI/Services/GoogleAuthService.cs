using Google.Apis.Auth.OAuth2;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Services.IServices;
using Microsoft.Extensions.Options;

namespace HospitalityCustomerAPI.Services
{
    public class GoogleAuthService: IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly GoogleAuthOptions _options;
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/firebase.messaging" };

        public GoogleAuthService(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IOptions<GoogleAuthOptions> options)
        {
            _configuration = configuration;
            _environment = environment;
            _options = options.Value;
        }


        public async Task<string> GetAccessTokenAsync()
        {
            // Sử dụng đường dẫn từ options nếu có, nếu không thì dùng đường dẫn mặc định
            var configPath = string.IsNullOrEmpty(_options.ServiceAccountPath)
                ? "Configurations/service-account.json"
                : _options.ServiceAccountPath;

            var serviceAccountPath = Path.Combine(_environment.ContentRootPath, configPath);

            try
            {
                if (!File.Exists(serviceAccountPath))
                {
                    throw new FileNotFoundException($"Service account file not found at: {serviceAccountPath}");
                }

                using var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read);
                var credential = GoogleCredential
                    .FromStream(stream)
                    .CreateScoped(Scopes);

                var token = await credential.UnderlyingCredential
                    .GetAccessTokenForRequestAsync();

                return token;
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Failed to get Google access token: {ex.Message}", ex);
            }
        }
    }
}
