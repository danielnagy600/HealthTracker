using HealthTracker.Application.Features.Schedule.Commands.AddActivity;
using HealthTracker.Application.Features.Schedule.Commands.DeleteActivity;
using HealthTracker.Application.Features.Schedule.Commands.UpdateActivity;
using HealthTracker.Application.Features.Schedule.Models;
using HealthTracker.Application.Features.Schedule.Queries.GetActivity;
using HealthTracker.Application.Features.Schedule.Queries.GetScheduleDay;
using HealthTracker.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthTracker.Api.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ScheduleController(IMediator mediator, ILogger<ScheduleController> logger) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DayScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayScheduleResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await mediator.Send(new GetScheduleDayQuery(date), ct);

    [HttpGet("activities/{id:guid}")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivityItem>> GetActivity(Guid id, CancellationToken ct) =>
        await mediator.Send(new GetActivityQuery(id), ct) is { } item ? item : NotFound();

    [HttpPost("activities")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivityItem>> AddActivity(SaveActivityRequest req, CancellationToken ct)
    {
        var item = await mediator.Send(
            new AddActivityCommand(req.Date, req.StartTime, req.EndTime, req.Title, req.Color, req.Note), ct);
        Log.ActivityCreated(logger, item.Id);
        return CreatedAtAction(nameof(GetActivity), new { id = item.Id }, item);
    }

    [HttpPut("activities/{id:guid}")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivityItem>> UpdateActivity(Guid id, SaveActivityRequest req, CancellationToken ct)
    {
        var item = await mediator.Send(
            new UpdateActivityCommand(id, req.Date, req.StartTime, req.EndTime, req.Title, req.Color, req.Note), ct);
        if (item is null)
        {
            Log.ActivityNotFound(logger, nameof(UpdateActivity), id);
            return NotFound();
        }

        Log.ActivityUpdated(logger, id);
        return item;
    }

    [HttpDelete("activities/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActivity(Guid id, CancellationToken ct)
    {
        if (!await mediator.Send(new DeleteActivityCommand(id), ct))
        {
            Log.ActivityNotFound(logger, nameof(DeleteActivity), id);
            return NotFound();
        }

        Log.ActivityDeleted(logger, id);
        return NoContent();
    }

    [HttpGet("colors")]
    [ProducesResponseType<string[]>(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetColors() => Ok(Enum.GetNames<ActivityColor>());
}
