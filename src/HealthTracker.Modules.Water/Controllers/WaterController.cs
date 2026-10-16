using HealthTracker.Modules.Water.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Water.Controllers;

[ApiController]
[Route("api/water")]
[Authorize]
public sealed class WaterController(IWaterService svc) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct) =>
        Ok(await svc.GetTodaySummaryAsync(ct));

    [HttpGet("reminder")]
    public async Task<IActionResult> GetReminder(CancellationToken ct) =>
        Ok(await svc.GetReminderAsync(ct));

    [HttpPost("intake")]
    public async Task<IActionResult> AddIntake(AddIntakeRequest req, CancellationToken ct)
    {
        if (req.AmountMl <= 0)
            return BadRequest("AmountMl must be positive.");

        var item = await svc.AddIntakeAsync(req, ct);
        return Created($"/api/water/intake/{item.Id}", item);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) =>
        Ok(await svc.GetSettingsAsync(ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest req, CancellationToken ct) =>
        Ok(await svc.UpdateSettingsAsync(req, ct));
}
