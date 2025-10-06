public sealed class EspOptions
{
    public string BaseUrl { get; set; } = default!;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "1234";
    public Dictionary<string, string> Pins { get; set; } = new();
}
