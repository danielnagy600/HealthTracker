using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Water.Controllers;

[ApiController]
[Route("api/water")]
[Authorize]
public sealed class WaterController(IWaterService svc) : ControllerBase
{
    [HttpGet("day")]
    public async Task<IActionResult> GetDay(DateOnly? date, CancellationToken ct) =>
        Ok(await svc.GetDayAsync(date, ct));

    [HttpGet("reminder")]
    public async Task<IActionResult> GetReminder(CancellationToken ct) =>
        Ok(await svc.GetReminderAsync(ct));

    [HttpGet("intakes/{id:guid}")]
    public async Task<IActionResult> GetIntake(Guid id, CancellationToken ct) =>
        await svc.GetIntakeAsync(id, ct) is { } item ? Ok(item) : NotFound();

    [HttpPost("intakes")]
    public async Task<IActionResult> AddIntake(AddIntakeRequest req, CancellationToken ct)
    {
        if (req.AmountMl <= 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: "AmountMl must be positive.");

        var item = await svc.AddIntakeAsync(req, ct);
        return CreatedAtAction(nameof(GetIntake), new { id = item.Id }, item);
    }

    [HttpDelete("intakes/{id:guid}")]
    public async Task<IActionResult> DeleteIntake(Guid id, CancellationToken ct) =>
        await svc.DeleteIntakeAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct) =>
        Ok(await svc.GetSettingsAsync(ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdateSettingsRequest req, CancellationToken ct) =>
        Ok(await svc.UpdateSettingsAsync(req, ct));
}
