using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("esp")]
public class EspTestController : ControllerBase
{
    private readonly IEspClient _esp;

    public EspTestController(IEspClient esp)
    {
        _esp = esp;
    }

    /// <summary>
    /// Test theo alias đã map trong appsettings (vd: gym_door_2).
    /// </summary>
    [HttpPost("test-alias")]
    public async Task<IActionResult> TestAlias([FromForm] string alias)
    {
        var ok = await _esp.TriggerByAliasAsync(alias);
        return Ok(new { success = ok, alias });
    }

    /// <summary>
    /// Test bắn raw pin token (vd: gym_door_2PULSE hoặc gym_door_2TOGGLE).
    /// </summary>
    [HttpPost("test-raw")]
    public async Task<IActionResult> TestRaw([FromForm] string pin)
    {
        var ok = await _esp.TriggerRawAsync(pin);
        return Ok(new { success = ok, pin });
    }
}
