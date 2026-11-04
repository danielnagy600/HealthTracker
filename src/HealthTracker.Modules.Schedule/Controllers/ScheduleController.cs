using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Application.Models;
using HealthTracker.Modules.Schedule.Domain;
using HealthTracker.Modules.Schedule.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthTracker.Modules.Schedule;

[ApiController]
[Route("api/schedule")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ScheduleController(IScheduleService svc, ILogger<ScheduleController> logger) : ControllerBase
{
    [HttpGet("day")]
    [ProducesResponseType<DayScheduleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DayScheduleResponse>> GetDay(DateOnly? date, CancellationToken ct) =>
        await svc.GetDayAsync(date, ct);

    [HttpGet("activities/{id:guid}")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivityItem>> GetActivity(Guid id, CancellationToken ct) =>
        await svc.GetAsync(id, ct) is { } item ? item : NotFound();

    [HttpPost("activities")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivityItem>> AddActivity(SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return Rejected(nameof(AddActivity), error);

        var item = await svc.AddAsync(req, ct);
        Log.ActivityCreated(logger, item.Id);
        return CreatedAtAction(nameof(GetActivity), new { id = item.Id }, item);
    }

    [HttpPut("activities/{id:guid}")]
    [ProducesResponseType<ActivityItem>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivityItem>> UpdateActivity(Guid id, SaveActivityRequest req, CancellationToken ct)
    {
        if (Validate(req) is { } error)
            return Rejected(nameof(UpdateActivity), error);

        var item = await svc.UpdateAsync(id, req, ct);
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
        if (!await svc.DeleteAsync(id, ct))
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

    private ObjectResult Rejected(string action, string reason)
    {
        Log.RequestRejected(logger, action, reason);
        return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: reason);
    }

    private static string? Validate(SaveActivityRequest req)
    {
        if (!ScheduleService.IsKnownColor(req.Color))
            return $"Unknown color: '{req.Color}'. Available: {string.Join(", ", Enum.GetNames<ActivityColor>())}.";

        return Activity.Validate(req.StartTime, req.EndTime, req.Title, req.Note);
    }
}
