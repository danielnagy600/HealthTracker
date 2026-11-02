using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Modules.Water.Controllers;

[ApiController]
[Route("api/water")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class WaterController(IWaterService svc) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DailySummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DailySummaryResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await svc.GetDayAsync(date, ct);

    [HttpGet("reminder")]
    [ProducesResponseType<ReminderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ReminderResponse>> GetReminder(CancellationToken ct) =>
        await svc.GetReminderAsync(ct);

    [HttpGet("intakes/{id:guid}")]
    [ProducesResponseType<IntakeItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntakeItem>> GetIntake(Guid id, CancellationToken ct) =>
        await svc.GetIntakeAsync(id, ct) is { } item ? item : NotFound();

    [HttpPost("intakes")]
    [ProducesResponseType<IntakeItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeItem>> AddIntake(AddIntakeRequest req, CancellationToken ct)
    {
        if (req.AmountMl <= 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: "AmountMl must be positive.");

        var item = await svc.AddIntakeAsync(req, ct);
        return CreatedAtAction(nameof(GetIntake), new { id = item.Id }, item);
    }

    [HttpDelete("intakes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIntake(Guid id, CancellationToken ct) =>
        await svc.DeleteIntakeAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("settings")]
    [ProducesResponseType<SettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SettingsResponse>> GetSettings(CancellationToken ct) =>
        await svc.GetSettingsAsync(ct);

    [HttpPut("settings")]
    [ProducesResponseType<SettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SettingsResponse>> UpdateSettings(UpdateSettingsRequest req, CancellationToken ct) =>
        await svc.UpdateSettingsAsync(req, ct);
}
