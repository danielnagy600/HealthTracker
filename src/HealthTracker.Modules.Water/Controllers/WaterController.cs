using HealthTracker.Modules.Water.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Water.Controllers;

// Controller-alapú megfelelője a WaterModule.MapWaterModule minimal API
// végpontjainak – ugyanazok az útvonalak, ugyanaz az IWaterService, csak
// attribútum-routing-gal és osztályba szervezve, nem lambdákkal.
[ApiController]
[Route("api/water")]
[Authorize]
public class WaterController : ControllerBase
{
    private readonly IWaterService _service;

    public WaterController(IWaterService service)
    {
        _service = service;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        return Ok(await _service.GetTodaySummaryAsync(ct));
    }

    [HttpGet("reminder")]
    public async Task<IActionResult> GetReminder(CancellationToken ct)
    {
        return Ok(await _service.GetReminderAsync(ct));
    }

    [HttpPost("intake")]
    public async Task<IActionResult> AddIntake([FromBody] AddIntakeRequest req, CancellationToken ct)
    {
        if (req.AmountMl <= 0)
            return BadRequest("AmountMl must be positive.");

        var item = await _service.AddIntakeAsync(req, ct);
        return Created($"/api/water/intake/{item.Id}", item);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        return Ok(await _service.GetSettingsAsync(ct));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest req, CancellationToken ct)
    {
        return Ok(await _service.UpdateSettingsAsync(req, ct));
    }
}
