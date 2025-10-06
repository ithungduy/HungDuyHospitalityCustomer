using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

public interface IEspClient
{
    /// <summary>
    /// Trigger theo alias (map sang chuỗi pin thật trong cấu hình).
    /// </summary>
    Task<bool> TriggerByAliasAsync(string alias, CancellationToken ct = default);

    /// <summary>
    /// Trigger trực tiếp bằng chuỗi pin (bỏ qua alias mapping).
    /// </summary>
    Task<bool> TriggerRawAsync(string pinToken, CancellationToken ct = default);
}

public sealed class EspClient : IEspClient
{
    private readonly EspOptions _opt;
    private readonly ILogger<EspClient> _logger;

    public EspClient(IOptions<EspOptions> opt, ILogger<EspClient> logger)
    {
        _opt = opt.Value;
        _logger = logger;
    }

    public Task<bool> TriggerByAliasAsync(string alias, CancellationToken ct = default)
    {
        if (!_opt.Pins.TryGetValue(alias, out var pinToken))
        {
            _logger.LogWarning("ESP alias '{alias}' chưa được cấu hình trong EspOptions:Pins", alias);
            return Task.FromResult(false);
        }
        return TriggerInternalAsync(pinToken, ct);
    }

    public Task<bool> TriggerRawAsync(string pinToken, CancellationToken ct = default)
        => TriggerInternalAsync(pinToken, ct);

    private async Task<bool> TriggerInternalAsync(string pinToken, CancellationToken ct)
    {
        // Dùng CookieContainer để giữ session_id giữa /login → /autologin → /api/trigger-gpio
        var cookies = new CookieContainer();
        using var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookies,
            AllowAutoRedirect = false
        };

        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(_opt.BaseUrl.TrimEnd('/')),
            Timeout = TimeSpan.FromSeconds(10)
        };

        try
        {
            // 1) LOGIN
            var loginForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("username", _opt.Username),
                new KeyValuePair<string,string>("password", _opt.Password),
            });

            var loginResp = await http.PostAsync("/login", loginForm, ct);
            if ((int)loginResp.StatusCode >= 500)
            {
                _logger.LogWarning("ESP /login thất bại: {Status}", loginResp.StatusCode);
                return false;
            }

            // 2) AUTOLOGIN (không bắt buộc phải 200)
            try
            {
                var al = await http.GetAsync("/autologin", ct);
                _logger.LogDebug("ESP /autologin → {Status}", al.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ESP /autologin lỗi/bỏ qua.");
            }

            // 3) TRIGGER
            var json = JsonSerializer.Serialize(new { pin = pinToken });
            var req = new StringContent(json, Encoding.UTF8, "application/json");

            var trig = await http.PostAsync("/api/trigger-gpio", req, ct);
            var content = await trig.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("ESP trigger '{pin}' → {Status} | {Body}", pinToken, trig.StatusCode, content);

            if (!trig.IsSuccessStatusCode)
                return false;

            // {"success": true, "message": "..."}
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("success", out var ok))
                    return ok.GetBoolean();
            }
            catch { /* parse lỗi thì coi như ok nếu HTTP 200 */ }

            return true;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("ESP request timeout");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ESP request lỗi");
            return false;
        }
    }
}
