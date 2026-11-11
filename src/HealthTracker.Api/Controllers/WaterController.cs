using HealthTracker.Application.Features.Water.Commands.AddIntake;
using HealthTracker.Application.Features.Water.Commands.DeleteIntake;
using HealthTracker.Application.Features.Water.Commands.UpdateWaterSettings;
using HealthTracker.Application.Features.Water.Models;
using HealthTracker.Application.Features.Water.Queries.GetIntake;
using HealthTracker.Application.Features.Water.Queries.GetWaterDay;
using HealthTracker.Application.Features.Water.Queries.GetWaterReminder;
using HealthTracker.Application.Features.Water.Queries.GetWaterSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Api.Controllers;

[ApiController]
[Route("api/water")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class WaterController(IMediator mediator, ILogger<WaterController> logger) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DailySummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DailySummaryResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await mediator.Send(new GetWaterDayQuery(date), ct);

    [HttpGet("reminder")]
    [ProducesResponseType<ReminderResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ReminderResponse>> GetReminder(CancellationToken ct) =>
        await mediator.Send(new GetWaterReminderQuery(), ct);

    [HttpGet("intakes/{id:guid}")]
    [ProducesResponseType<IntakeItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IntakeItem>> GetIntake(Guid id, CancellationToken ct) =>
        await mediator.Send(new GetIntakeQuery(id), ct) is { } item ? item : NotFound();

    [HttpPost("intakes")]
    [ProducesResponseType<IntakeItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IntakeItem>> AddIntake(AddIntakeRequest req, CancellationToken ct)
    {
        var item = await mediator.Send(new AddIntakeCommand(req.AmountMl), ct);
        Log.IntakeAdded(logger, item.Id);
        return CreatedAtAction(nameof(GetIntake), new { id = item.Id }, item);
    }

    [HttpDelete("intakes/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIntake(Guid id, CancellationToken ct)
    {
        if (!await mediator.Send(new DeleteIntakeCommand(id), ct))
        {
            Log.IntakeNotFound(logger, nameof(DeleteIntake), id);
            return NotFound();
        }

        Log.IntakeDeleted(logger, id);
        return NoContent();
    }

    [HttpGet("settings")]
    [ProducesResponseType<SettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SettingsResponse>> GetSettings(CancellationToken ct) =>
        await mediator.Send(new GetWaterSettingsQuery(), ct);

    [HttpPut("settings")]
    [ProducesResponseType<SettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SettingsResponse>> UpdateSettings(UpdateSettingsRequest req, CancellationToken ct)
    {
        var settings = await mediator.Send(
            new UpdateWaterSettingsCommand(req.DailyTargetMl, req.WakeTime, req.SleepTime), ct);
        Log.WaterSettingsUpdated(logger);
        return settings;
    }
}
