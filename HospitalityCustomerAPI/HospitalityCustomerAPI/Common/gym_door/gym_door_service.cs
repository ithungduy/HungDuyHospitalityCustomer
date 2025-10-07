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

    // 🆕 dùng IP & pin lấy từ điểm bán hàng
    Task<bool> TriggerWithEndpointAsync(
        string baseUrl,             // ví dụ "http://172.16.10.169"
        string pinToken,            // ví dụ "gym_door_2PULSE"
        string? username = null,    // null => dùng username từ cấu hình global
        string? password = null,    // null => dùng password từ cấu hình global
        CancellationToken ct = default);
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
        if (_opt.Pins.TryGetValue(alias, out var pinToken))
            return TriggerInternalAsync(_opt.BaseUrl, pinToken, _opt.Username, _opt.Password, ct);

        if (_opt.Pins.Values.Contains(alias)) // alias trùng giá trị pin
            return TriggerInternalAsync(_opt.BaseUrl, alias, _opt.Username, _opt.Password, ct);

        _logger.LogWarning("ESP alias '{alias}' không có trong cấu hình.", alias);
        return Task.FromResult(false);
    }

    public Task<bool> TriggerRawAsync(string pinToken, CancellationToken ct = default)
        => TriggerInternalAsync(_opt.BaseUrl, pinToken, _opt.Username, _opt.Password, ct);

    // 🆕 truyền IP & pin từ điểm bán
    public Task<bool> TriggerWithEndpointAsync(
        string baseUrl,
        string pinToken,
        string? username = null,
        string? password = null,
        CancellationToken ct = default)
    {
        var u = string.IsNullOrWhiteSpace(username) ? _opt.Username : username!;
        var p = string.IsNullOrWhiteSpace(password) ? _opt.Password : password!;
        return TriggerInternalAsync(baseUrl, pinToken, u, p, ct);
    }

    // ===== core HTTP flow: login -> autologin -> trigger =====
    private async Task<bool> TriggerInternalAsync(
        string baseUrl,
        string pinToken,
        string username,
        string password,
        CancellationToken ct)
    {
        var cookies = new CookieContainer();
        using var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookies,
            AllowAutoRedirect = false
        };

        using var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/')),
            Timeout = TimeSpan.FromSeconds(10)
        };

        try
        {
            // 1) /login
            var loginForm = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("username", username),
                new KeyValuePair<string,string>("password", password),
            });
            var loginResp = await http.PostAsync("/login", loginForm, ct);
            if (!loginResp.IsSuccessStatusCode && (int)loginResp.StatusCode >= 500)
            {
                _logger.LogWarning("ESP /login thất bại: {Status} {BaseUrl}", loginResp.StatusCode, baseUrl);
                return false;
            }

            // 2) /autologin (best-effort)
            try
            {
                var al = await http.GetAsync("/autologin", ct);
                _logger.LogDebug("ESP /autologin → {Status} {BaseUrl}", al.StatusCode, baseUrl);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ESP /autologin lỗi/bỏ qua. {BaseUrl}", baseUrl);
            }

            // 3) /api/trigger-gpio
            var json = JsonSerializer.Serialize(new { pin = pinToken });
            var req = new StringContent(json, Encoding.UTF8, "application/json");
            var trig = await http.PostAsync("/api/trigger-gpio", req, ct);
            var content = await trig.Content.ReadAsStringAsync(ct);

            _logger.LogInformation("ESP trigger {pin} → {Status} | {Body}", pinToken, trig.StatusCode, content);

            if (!trig.IsSuccessStatusCode) return false;

            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("success", out var ok))
                    return ok.GetBoolean();
            }
            catch { /* nếu JSON khác format, coi 200 là ok */ }

            return true;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("ESP timeout {BaseUrl}", baseUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ESP exception {BaseUrl}", baseUrl);
            return false;
        }
    }
}

